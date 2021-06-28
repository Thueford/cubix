using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using ParticleProps;
using RenderSettings = ParticleProps.RenderSettings;

// [ExecuteAlways]
public class Particles : MonoBehaviour
{
    #region General
    [Header("General")]
    public Stats stats;
    public GeneralProps properties = GeneralProps.dflt;
    public RenderSettings renderSettings = RenderSettings.Default;

    [Header("Emission")]
    [Tooltip("xy: offset, z: timefac")]
    public Capsule<Vector3> size = new Capsule<Vector3>(new Vector3(.2f, .2f, 0));
    public DynamicEffect pos = new DynamicEffect(Vector3.one, Vector3.zero, Shape.SPHERE);
    public DynamicEffect vel = new DynamicEffect(Vector3.zero, Vector3.zero, Shape.SPHERE);
    public DynamicEffect force = new DynamicEffect(Vector3.zero, Vector3.zero, Shape.SPHERE);
    public DynamicEffect posFac = new DynamicEffect(Vector3.one, Vector3.zero, Shape.SPHERE);

    [Header("Other")]
    public Colors color = Colors.dflt;

    #region Flags
    private static int F(bool v, int p) => v ? 1 << p : 0;
    private static int F(int v, int p) => v * 1 << p;

    int GetFlags()
    {
        return
            F(!stats.prewarmed, 0) +
            F(color.useGradient, 1) +
            F(color.useVariation, 2) +
            0;
    }
    #endregion

    #endregion

    #region Privates
    private float partTimer = 0; // for spawning new particles
    private Vector3 lastPos;

    private int curMaxParts = 1;
    private Vector2[] meshVerts;
    private static bool editorDrawing = false;

    private bool isEditorPlaying(EditorDrawMode edm) { return isAnimPlaying() && stats.editorDrawMode == edm; }
    private bool isAnimPlaying() { return Application.isPlaying || (editorDrawing && !EditorApplication.isPaused); }
    private bool isAnimPaused() { return !Application.isPlaying && (!editorDrawing || (EditorApplication.isPaused || stats.editorDrawMode == 0)); }

    #endregion

    #region Unity

    void Awake()
    {
        if (tex == null && mat.mainTexture != null) tex = mat.mainTexture;
        stats = new Stats();
        stats.editorDrawMode = EditorDrawMode.OFF;
        if(isAnimPlaying()) Initialize();
    }

    private void OnDrawGizmosSelected()
    {
        // pos.Correct(); vel.Correct(); force.Correct(); posFac.Correct();
        renderSettings.setPreset();

        if (!editorDrawing) return;
        if (isAnimPaused()) { ReleaseBuffers(); return; }

        //update maxParts
        if (curMaxParts != properties.maxParts) {
            curMaxParts = properties.maxParts;
            ReleaseBuffers(); Initialize();
        }

        if (stats.reset) ReleaseBuffers();

        if (isEditorPlaying(EditorDrawMode.FAST)) {
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (editorDrawing && isEditorPlaying(EditorDrawMode.SLOW))
            EditorApplication.delayCall += EditorApplication.QueuePlayerLoopUpdate;
        if (isAnimPaused()) return;

        DispatchUpdate();

        if (Application.isPlaying && !properties.repeat && stats.emitted >= curMaxParts && stats.alive == 0)
        {
            if (_onFinished != null) _onFinished();
            if (properties.destroyOnFinished) Destroy(gameObject);
            return;
        }

        // spawn particles
        if (properties.enabled && properties.emissionRate > 1e-2)
        {
            if (partTimer == 0) partTimer = -10*Time.deltaTime;
            partTimer += Time.deltaTime;

            DispatchEmit((int)(partTimer * properties.emissionRate));
            partTimer -= (int)(partTimer * properties.emissionRate) / properties.emissionRate;
        }
    }

    void OnRenderObject()
    {
        if (isAnimPaused()) return;
        if (!stats.initialized) Initialize();
        mat.mainTexture = tex;

        // set uniforms
        mat.SetBuffer("_Particles", particlesBuf);
        mat.SetBuffer("_QuadVert", quadVertBuf);
        renderSettings.Uniform(mat);

        // mat.SetFloat("_Time", Time.time);
        mat.SetPass(0);

        if (deadBuf == null) Debug.LogWarning("... " + stats.initialized);
        else Graphics.DrawProceduralNow(MeshTopology.Triangles, meshVerts.Length, deadBuf.count);
    }

    private void OnDisable() { ReleaseBuffers(); }
    private void OnDestroy() { ReleaseBuffers(); }

    #endregion

    #region User
    [Range(-10, 10)]
    public float velocityFactor = 0;
    //public Vector3 posOffset;
    //public Quaternion rotation;
    private System.Action _onFinished;

    public void Initialize()
    {
        ShaderSetup();
        InitializePartBuffer();
        DispatchInit();

        stats.initialized = true;

        ResetPS();
    }

    public void ResetPS()
    {
        deadBuf.SetCounterValue((uint)curMaxParts);
        ReadDeadCount();
        stats.alive = 0;
        stats.dead = curMaxParts;
        stats.emitted = 0;

        if (properties.prewarm)
        {
            int count = (int)(properties.emissionRate * properties.lifetime);
            count = Mathf.Min(properties.maxParts, count);
            DispatchEmit(count);
            DispatchUpdate();
        }

        stats.prewarmed = true;
    }

    public void Play()
    {
        properties.enabled = true;
    }

    public void Stop()
    {
        properties.enabled = false;
    }
    public void SetOnFinished(System.Action p)
    {
        _onFinished = p;
    }

    #endregion

    #region Shader
    // public Assets assets; // not visible in script inspector :(
    [NotNull] public Texture tex;
    [NotNull] public Material mat;
    [NotNull] public ComputeShader compute;
    private ComputeBuffer particlesBuf, deadBuf;
    private ComputeBuffer counterBuf, quadVertBuf;

    private int kernelInit, kernelEmit, kernelUpdate;


    #region Shader Setup

    private void ShaderSetup()
    {
        kernelInit = compute.FindKernel("Init");
        kernelEmit = compute.FindKernel("EmitOne");
        kernelUpdate = compute.FindKernel("Update");
        compute.GetKernelThreadGroupSizes(kernelInit, out uint threads, out _, out _);

        curMaxParts = properties.maxParts;
        stats.groupCount = Mathf.CeilToInt((float)curMaxParts / threads);
        stats.bufferSize = stats.groupCount * (int)threads;

        if (curMaxParts < (1 << 14))
        {
            stats.bufferSize = stats.groupCount = curMaxParts;
            kernelInit = compute.FindKernel("InitOne");
            kernelUpdate = compute.FindKernel("UpdateOne");
        }

        meshVerts = new[] {
            new Vector2(-0.5f,0.5f),
            new Vector2(0.5f,0.5f),
            new Vector2(0.5f,-0.5f),
            new Vector2(0.5f,-0.5f),
            new Vector2(-0.5f,-0.5f),
            new Vector2(-0.5f,0.5f),
        };
    }

    private void InitializePartBuffer()
    {
        particlesBuf = new ComputeBuffer(stats.bufferSize, Marshal.SizeOf<Particle>());

        deadBuf = new ComputeBuffer(stats.bufferSize, sizeof(int), ComputeBufferType.Append);
        deadBuf.SetCounterValue(0);

        counterBuf = new ComputeBuffer(counterArray.Length, sizeof(int), ComputeBufferType.IndirectArguments);
        counterBuf.SetData(counterArray);

        quadVertBuf = new ComputeBuffer(meshVerts.Length, Marshal.SizeOf<Vector2>());
        quadVertBuf.SetData(meshVerts);
    }


    private void DispatchInit()
    {
        compute.SetBuffer(kernelInit, "_Particles", particlesBuf);
        compute.SetBuffer(kernelInit, "_Dead", deadBuf);
        compute.Dispatch(kernelInit, stats.groupCount, 1, 1);
        ReadDeadCount();
    }

    private void ReleaseBuffers()
    {
        if (!stats.initialized) return;
        EditorDrawMode edm = stats.editorDrawMode;
        stats = new Stats();
        stats.editorDrawMode = edm;

        if (particlesBuf != null) particlesBuf.Release();
        if (quadVertBuf != null) quadVertBuf.Release();
        if (counterBuf != null) counterBuf.Release();
        if (deadBuf != null) deadBuf.Release();
    }

    #endregion

    #region Shader Loop

    private void UniformEmit(int kernel)
    {
        Vector3 velocity = (transform.position - lastPos); // / Time.deltaTime;
        lastPos = transform.position;

        compute.SetBuffer(kernel, "_Particles", particlesBuf);
        compute.SetBuffer(kernel, "_Alive", deadBuf);

        compute.SetInt("_Flags", GetFlags());
        compute.SetVector("_PosParent", transform.position); // + posOffset
        // compute.SetMatrix("_Rotation", Matrix4x4.Rotate(rotation));
        compute.SetVector("_SpdParent", velocityFactor * velocity);
        compute.SetVector("_Seeds", new Vector4(Random.value, Random.value, Random.value, Random.value));
        compute.SetFloat("_Lifetime", properties.lifetime);

        pos.Uniform(compute, "_Pos");
        vel.Uniform(compute, "_Spd");
        force.Uniform(compute, "_Force");
        posFac.Uniform(compute, "_PosFac");

        compute.SetVector("_Size", size.val);
        color.UniformEmit(compute, "_Color");
    }

    public int DispatchEmit(int count)
    {
        count = Mathf.Min(count, 1<<15, curMaxParts - stats.alive);
        if (!properties.repeat && stats.emitted + count > properties.maxParts)
            count = properties.maxParts - stats.emitted;

        if (count <= 0) count = 0;
        stats.ppf = count;
        stats.pps = Mathf.RoundToInt(count / Time.deltaTime);
        if (count <= 0) return 0;

        stats.emitted += count;

        UniformEmit(kernelEmit);
        compute.Dispatch(kernelEmit, count, 1, 1);
        return count;
    }

    private void UniformUpdate(int kernel)
    {
        compute.SetBuffer(kernel, "_Particles", particlesBuf);
        compute.SetBuffer(kernel, "_Dead", deadBuf);
        color.Uniform(compute, kernel, "_Color");

        compute.SetInt("_Flags", GetFlags());
        compute.SetFloat("_SizeVel", size.val.z);
        compute.SetFloat("_DeltaTime", stats.initialized ? Time.deltaTime : Time.fixedDeltaTime);
    }

    private void DispatchUpdate()
    {
        if (!stats.initialized || particlesBuf == null) Initialize();
        UniformUpdate(kernelUpdate);
        compute.Dispatch(kernelUpdate, stats.groupCount, 1, 1);
        ReadDeadCount();
    }

    private static int[] counterArray = new int[1];
    private void ReadDeadCount()
    {
        ComputeBuffer.CopyCount(deadBuf, counterBuf, 0);
        counterBuf.GetData(counterArray);
        stats.dead = counterArray[0];
        stats.alive = curMaxParts - stats.dead;
    }

    #endregion

    #endregion
}