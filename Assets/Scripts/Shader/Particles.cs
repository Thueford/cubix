using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Particles {
public class Particles : MonoBehaviour
{
    #region General
    [Header("General")]
    public Stats stats;
    public GeneralProps properties = GeneralProps.dflt;
    public RenderSettigns renderSettings = RenderSettigns.dflt;

    [Header("Emission")]
    [Tooltip("xy: offset, z: timefac")]
    public Capsule<Vector3> size = new Capsule<Vector3>(Vector3.one);
    public DynamicEffect pos;
    public DynamicEffect vel;
    public DynamicEffect force;
    
    [Header("Other")]
    //public Vector2 size = new Vector2(0.2f, 0.2f);
    public Color color = Color.yellow;

    #region Flags
    private static int F(bool v, int p) { return v ? 1 << p : 0; }
    private static int F(int v, int p) { return v * 1 << p; }

    int GetFlags()
    {
        return 
            F(pos.shape == Shape.SPHERE, 0) +
            F(vel.shape == Shape.SPHERE, 1) +
            F(force.shape == Shape.SPHERE, 2) +
            0;
    }
        #endregion

    #endregion

    #region Privates
    private float partTimer = 0, timePerPart; // for spawning new particles
    private Vector3 lastPos;

    // private Particle[] particles;
    private int curMaxParts = 1;
    private Vector2[] meshVerts;

    [NotNull] public Material mat;
    // [NotNull] public Mesh mesh;

    #endregion

    #region Unity

    void Awake()
    {
        // mat.enableInstancing = true;
        ShaderSetup();
    }

    // Update is called once per frame
    void Update()
    {
        DispatchUpdate();

        /* //update maxParts
        if (curMaxParts != maxParts) {
            curMaxParts = maxParts;
            particles = new Particle[maxParts];
        } */

        // spawn particles
        timePerPart = 1 / properties.emissionRate;
        if (partTimer == 0) partTimer = -10*Time.deltaTime;
        partTimer += Time.deltaTime;
        partTimer -= timePerPart * DispatchEmit((int)(partTimer / timePerPart));
    }

    void OnRenderObject()
    {
        // set uniforms
        mat.SetBuffer("_Particles", particlesBuf);
        mat.SetBuffer("_QuadVert", quadVertBuf);
        renderSettings.Uniform(mat);

        // mat.SetFloat("_Time", Time.time);
        mat.SetPass(0);

        Graphics.DrawProceduralNow(MeshTopology.Triangles, meshVerts.Length, deadBuf.count);
    }
    #endregion

    #region Shader

    public ComputeShader compute;
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
        stats.groupCount = Mathf.CeilToInt((float)properties.maxParts / threads);
        stats.bufferSize = stats.groupCount * (int)threads;

        if (properties.maxParts < (1 << 14))
        {
            stats.bufferSize = stats.groupCount = properties.maxParts;
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

        InitializePartBuffer();
        DispatchInit();
    }

    private void InitializePartBuffer()
    {
        ReleaseBuffers();

        particlesBuf = new ComputeBuffer(stats.bufferSize, Marshal.SizeOf<Particle>());

        deadBuf = new ComputeBuffer(stats.bufferSize, sizeof(int), ComputeBufferType.Append);
        deadBuf.SetCounterValue(0);

        counterBuf = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        counterArray = new int[] { 0, 1, 0, 0 };

        quadVertBuf = new ComputeBuffer(meshVerts.Length, Marshal.SizeOf<Vector2>());
        quadVertBuf.SetData(meshVerts);
    }


    private void DispatchInit()
    {
        compute.SetBuffer(kernelInit, "_Particles", particlesBuf);
        compute.SetBuffer(kernelInit, "_Dead", deadBuf);
        compute.Dispatch(kernelInit, stats.groupCount, 1, 1);
    }

    private void ReleaseBuffers()
    {
        if (particlesBuf != null) particlesBuf.Release();
        if (quadVertBuf != null) quadVertBuf.Release();
        if (counterBuf != null) counterBuf.Release();
        if (deadBuf != null) deadBuf.Release();
    }

    #endregion

    #region Shader Loop

    public int DispatchEmit(int count)
    {
        count = Mathf.Min(count, 1<<15, properties.maxParts - (stats.bufferSize - stats.dead));
        if (count <= 0) return 0;

        Vector3 velocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;

        compute.SetBuffer(kernelEmit, "_Particles", particlesBuf);
        compute.SetBuffer(kernelEmit, "_Alive", deadBuf);

        compute.SetInt("_Flags", GetFlags());
        compute.SetVector("_Seeds", new Vector4(Random.value, Random.value, Random.value, Random.value));
        compute.SetFloat("_Lifetime", properties.lifetime);

        pos.Uniform(compute, "_Pos");
        vel.Uniform(compute, "_Spd");
        force.Uniform(compute, "_Force");

        compute.SetVector("_Size", size.val);
        compute.SetVector("_Color", color);

        compute.Dispatch(kernelEmit, count, 1, 1);
        return count;
    }

    private void DispatchUpdate()
    {
        compute.SetFloat("_SizeVel", size.val.z);
        compute.SetFloat("_DeltaTime", Time.deltaTime);
        compute.SetBuffer(kernelUpdate, "_Particles", particlesBuf);
        compute.SetBuffer(kernelUpdate, "_Dead", deadBuf);

        compute.Dispatch(kernelUpdate, stats.groupCount, 1, 1);
        ReadDeadCount();
    }

    private int[] counterArray;
    private void ReadDeadCount()
    {
        if (deadBuf == null || counterBuf == null || counterArray == null)
        {
            stats.dead = stats.bufferSize;
            return;
        }
        counterBuf.SetData(counterArray);
        ComputeBuffer.CopyCount(deadBuf, counterBuf, 0);
        counterBuf.GetData(counterArray);
        stats.dead = counterArray[0];
        stats.alive = properties.maxParts - stats.dead;
    }

    #endregion

    private void OnDisable() {
        ReleaseBuffers();
    }
    private void OnDestroy() {
        ReleaseBuffers();
    }
    #endregion
}
}