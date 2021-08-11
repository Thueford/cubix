using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using ParticleProps;
using RenderSettings = ParticleProps.RenderSettings;
using static Helper;

// [ExecuteAlways]
public class Particles : MonoBehaviour
{
    #region Variables

    #region Publics
    [Header("General")]
    public Stats stats;
    public GeneralProps properties = GeneralProps.dflt;
    public RenderSettings renderSettings = RenderSettings.Default;

    [Header("Particles")]
    [Tooltip("xy: offset, z: timefac")]
    public Capsule<Vector3> size = new Capsule<Vector3>(new Vector3(.2f, .2f, 0));
    public DynamicEffect pos = new DynamicEffect(Vector3.one, Vector3.zero, Shape.SPHERE);
    public DynamicEffect vel = new DynamicEffect(Vector3.zero, Vector3.zero, Shape.SPHERE);
    public DynamicEffect force = new DynamicEffect(Vector3.zero, Vector3.zero, Shape.SPHERE);
    public DynamicEffect posFac = new DynamicEffect(Vector3.one, Vector3.zero, Shape.SPHERE);
    //public DynamicEffect radial = new DynamicEffect(Vector3.zero, Vector3.zero, Shape.SPHERE);

    [Tooltip("Vector3 + w: factor")]
    public Vector4 attractor;
    public Colors color = Colors.dflt;
    #endregion

    #region Flags
    int GetFlags() =>
        F(!stats.prewarmed, 0) +
        F(color.useGradient, 1) +
        F(color.useVariation, 2);
    #endregion

    #region Statics
    public static bool enableParticles = true;
    private static float deadUpdDelay = 1;

    private int curMaxParts = 1;
    private static Vector2[] meshVerts = new[] {
            new Vector2(-0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,-0.5f),
            new Vector2(0.5f,-0.5f), new Vector2(-0.5f,-0.5f), new Vector2(-0.5f,0.5f),
        };
    private static bool editorDrawing = false;
    #endregion

    #region Privates
    private float _alive;
    private float partTimer = 0; // for spawning new particles
    private Vector3 lastPos, _velocity;

    private bool isEditorPlaying(EditorDrawMode edm) { return isAnimPlaying() && stats.editorDrawMode == edm; }

    #if UNITY_EDITOR
        private bool isAnimPlaying() { return Application.isPlaying || (editorDrawing && !EditorApplication.isPaused); }
        private bool isAnimPaused() { return !Application.isPlaying && (!editorDrawing || (EditorApplication.isPaused || stats.editorDrawMode == 0)); }
    #else
        private bool isAnimPlaying() { return Application.isPlaying; }
        private bool isAnimPaused() { return !Application.isPlaying; }
    #endif

    #endregion

    #endregion

    #region Functions

    #region Unity

    void Start()
    {
        if (!enableParticles) return;
        if (tex == null && mat.mainTexture != null) tex = mat.mainTexture;
        stats = new Stats();
        stats.editorDrawMode = EditorDrawMode.OFF;
        SetEnabled(properties.enabled);
        if (isAnimPlaying()) Initialize();
    }

    private void OnDrawGizmosSelected()
    {
        if (!enableParticles || !editorDrawing) return;
        if (isAnimPaused()) { ReleaseBuffers(); return; }
        renderSettings.setPreset();

        //update maxParts
        if (curMaxParts != properties.maxParts) {
            curMaxParts = properties.maxParts;
            ReleaseBuffers(); Initialize();
        }

        if (stats.reset) ReleaseBuffers();

        #if UNITY_EDITOR
            if (isEditorPlaying(EditorDrawMode.FAST)) {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        #if UNITY_EDITOR
            if (editorDrawing && isEditorPlaying(EditorDrawMode.SLOW))
                EditorApplication.delayCall += EditorApplication.QueuePlayerLoopUpdate;
        #endif

        if (!enableParticles || isAnimPaused()) return;
        if (stats.reset) { stats.reset = false; ResetPS(); }
        
        _velocity = (transform.position - lastPos); // / Time.deltaTime;
        lastPos = transform.position;

        DispatchUpdate();

        if (properties.performance > PerformanceMode.LOW)
        {
            _alive -= Time.deltaTime * Mathf.Ceil(stats.alive) / properties.lifetime;
            stats.alive = Mathf.CeilToInt(_alive);
            stats.dead = curMaxParts - stats.alive;
        }

        if (Application.isPlaying && !properties.repeat && stats.emitted >= curMaxParts && stats.alive == 0)
        {
            if (_onFinished != null) _onFinished();
            if (properties.destroyOnFinished) Destroy(gameObject);
            return;
        }

        // spawn particles
        if (properties.enabled && properties.emissionRate > 1e-2)
        {
            if (partTimer == 0) partTimer = -10 * Time.deltaTime;
            partTimer += Time.deltaTime;

            DispatchEmit((int)(partTimer * properties.emissionRate));
            partTimer -= (int)(partTimer * properties.emissionRate) / properties.emissionRate;
        }
    }

    void OnRenderObject()
    {
        if (!enableParticles || isAnimPaused()) return;
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
        if (!enableParticles || deadBuf == null) return;
        deadBuf.SetCounterValue((uint)curMaxParts);
        if (properties.performance > PerformanceMode.LOW) 
            ReadDeadCount();
        _alive = stats.alive = 0;
        stats.dead = curMaxParts;
        stats.emitted = 0;
        tLastCount = -1;

        if (properties.prewarm)
        {
            int count = (int)(properties.emissionRate * properties.lifetime);
            DispatchEmit(Mathf.Min(curMaxParts, count));
            DispatchUpdate();
        }

        stats.prewarmed = true;
    }

    public void SetEnabled(bool b)
    {
        properties.enabled = b;
    }

    public void SetOnFinished(System.Action p)
    {
        _onFinished = p;
    }

    #endregion

    #endregion

    #region Shader

    #region Variables
    // public Assets assets; // not visible in script inspector :(
    [NotNull] public Texture tex;  // Particle Texture
    [NotNull] public Material mat; // Unlit Shader
    [NotNull] public ComputeShader compute;
    private ComputeBuffer particlesBuf; // structured Particle buffer
    private ComputeBuffer deadBuf;      // Append/Consume dead part id buffer
    private ComputeBuffer counterBuf;   // CounterBuffer
    private ComputeBuffer quadVertBuf;  // constant VertexData buffer
    private static int[] counterArray = new int[1];

    private int kernelInit, kernelEmit, kernelUpdate;
    #endregion


    #region Init

    private void ShaderSetup()
    {
        kernelInit = compute.FindKernel("Init");
        kernelEmit = compute.FindKernel("Emit");
        kernelUpdate = compute.FindKernel("Update");
        compute.GetKernelThreadGroupSizes(kernelInit, out uint threads, out _, out _);

        curMaxParts = properties.maxParts;
        stats.groupCount = Mathf.CeilToInt((float)curMaxParts / threads);
        stats.bufferSize = stats.groupCount * (int)threads;
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
        if (properties.performance == PerformanceMode.LOW) ReadDeadCount();
    }

    #endregion

    #region Emit

    private void UniformEmit(int kernel)
    {
        compute.SetBuffer(kernel, "_Particles", particlesBuf);
        compute.SetBuffer(kernel, "_Alive", deadBuf);

        compute.SetInt("_Flags", GetFlags());
        compute.SetVector("_PosParent", transform.position); // + posOffset
        // compute.SetMatrix("_Rotation", Matrix4x4.Rotate(rotation));
        compute.SetVector("_SpdParent", velocityFactor * _velocity);
        compute.SetVector("_Seeds", new Vector4(Random.value, Random.value, Random.value, Random.value));
        compute.SetFloat("_Lifetime", properties.lifetime);

        pos.Uniform(compute, "_Pos");
        vel.Uniform(compute, "_Spd");
        force.Uniform(compute, "_Force");
        posFac.Uniform(compute, "_PosFac");
        //radial.Uniform(compute, "_Radial");

        compute.SetVector("_Size", size.val);
        color.UniformEmit(compute, "_Color");
    }

    public int DispatchEmit(int count)
    {
        if (!properties.repeat && stats.emitted > curMaxParts) return 0;

        if (stats.medAlive >= curMaxParts && properties.repeat && stats.alive + count > curMaxParts)
            ReadDeadCount(true);
        
        count = Mathf.Min(count, 1 << 15, curMaxParts - stats.alive);
        if (!properties.repeat && stats.emitted + count > curMaxParts)
            count = curMaxParts - stats.emitted;

        if (count <= 0) count = 0;
        stats.ppf = count;
        stats.pps = Mathf.RoundToInt(count / Time.deltaTime);
        if (count <= 0) return 0;

        stats.medAlive = properties.lifetime * properties.emissionRate;
        stats.emitted += count;
        if (properties.performance > PerformanceMode.LOW)
        {
            stats.alive += count;
            _alive += count;
            stats.dead = curMaxParts - stats.alive;
        }

        UniformEmit(kernelEmit);
        compute.Dispatch(kernelEmit, count, 1, 1);
        return count;
    }
    #endregion

    #region Update
    private void UniformUpdate(int kernel)
    {
        compute.SetBuffer(kernel, "_Particles", particlesBuf);
        compute.SetBuffer(kernel, "_Dead", deadBuf);
        color.Uniform(compute, kernel, "_Color");

        compute.SetInt("_Flags", GetFlags());
        compute.SetVector("_PosParent", transform.position);
        compute.SetVector("_SpdParent", _velocity * velocityFactor);
        compute.SetVector("_Attractor", attractor);
        compute.SetFloat("_SizeVel", size.val.z);
        compute.SetFloat("_DeltaTime", stats.initialized ? Time.deltaTime : Time.fixedDeltaTime);
    }

    private void DispatchUpdate()
    {
        if (!stats.initialized || particlesBuf == null) Initialize();
        UniformUpdate(kernelUpdate);
        compute.Dispatch(kernelUpdate, stats.groupCount, 1, 1);
        if (properties.performance <= PerformanceMode.MEDIUM)
            ReadDeadCount();
    }
    #endregion

    #region General

    private float tLastCount = -1;
    private void ReadDeadCount(bool force = false)
    {
        if (!(properties.performance == PerformanceMode.LOW ||
            force || Time.time - tLastCount >= deadUpdDelay)) return;
        tLastCount = Time.time;

        ComputeBuffer.CopyCount(deadBuf, counterBuf, 0);
        counterBuf.GetData(counterArray);
        stats.dead = counterArray[0];
        stats.alive = curMaxParts - stats.dead;
        _alive = stats.alive;
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
        particlesBuf = quadVertBuf = counterBuf = deadBuf = null;
    }

    #endregion

    #endregion
}
