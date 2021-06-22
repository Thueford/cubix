using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Particles : MonoBehaviour
{
    #region General
    [Header("ParticleSystem Properties")]
    public int maxParts = 1;
    public float lifetime = 3;

    public float emissionRate = 5;
    public float startDelay = 0;
    public float range = 5;
    private Bounds bounds;

    public Vector2 size = new Vector2(0.2f, 0.2f);
    public Vector3 speed = Vector3.one;
    public Color color = Color.yellow;
    #endregion

    #region Other
    [Header("Other")]
    //[ReadOnly] public int curParts = 0;
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
        timePerPart = 1 / emissionRate;
        if (partTimer == 0) partTimer = -10*Time.deltaTime;
        partTimer += Time.deltaTime;
        partTimer -= timePerPart * DispatchEmit((int)(partTimer / timePerPart));
    }

    void OnRenderObject()
    {
        // set uniforms
        mat.SetBuffer("_Particles", particlesBuf);
        mat.SetBuffer("_QuadVert", quadVertBuf);
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
    int groupCount, bufferSize = (int)1e5;

    #region Shader Setup
    private void ShaderSetup()
    {
        kernelInit = compute.FindKernel("Init");
        kernelEmit = compute.FindKernel("EmitOne");
        kernelUpdate = compute.FindKernel("Update");
        compute.GetKernelThreadGroupSizes(kernelInit, out uint threads, out _, out _);
        groupCount = Mathf.CeilToInt((float)maxParts / threads);
        bufferSize = groupCount * (int)threads;

        if (groupCount < 1 << 12)
        {
            bufferSize = groupCount = maxParts;
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

        particlesBuf = new ComputeBuffer(bufferSize, Marshal.SizeOf<Particle>());

        deadBuf = new ComputeBuffer(bufferSize, sizeof(int), ComputeBufferType.Append);
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
        compute.Dispatch(kernelInit, groupCount, 1, 1);
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
        count = Mathf.Min(count, 1<<15, maxParts - (bufferSize - deadCount));
        if (count <= 0) return 0;

        Vector3 velocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;

        compute.SetBuffer(kernelEmit, "_Particles", particlesBuf);
        compute.SetBuffer(kernelEmit, "_Alive", deadBuf);

        compute.SetVector("_Seeds", new Vector4(Random.value, Random.value, Random.value, Random.value));
        compute.SetVector("_Speed", speed);
        compute.SetVector("_ParentPosition", transform.position);
        compute.SetFloat("_Lifetime", lifetime);
        compute.SetFloat("_Range", range);
        compute.SetVector("_Color", color);

        compute.Dispatch(kernelEmit, count, 1, 1);
        return count;
    }

    private void DispatchUpdate()
    {
        compute.SetFloat("_DeltaTime", Time.deltaTime);
        compute.SetBuffer(kernelUpdate, "_Particles", particlesBuf);
        compute.SetBuffer(kernelUpdate, "_Dead", deadBuf);

        compute.Dispatch(kernelUpdate, groupCount, 1, 1);
        ReadDeadCount();
    }

    [ReadOnly] public int deadCount = 0;
    private int[] counterArray;
    private void ReadDeadCount()
    {
        if (deadBuf == null || counterBuf == null || counterArray == null)
        {
            deadCount = bufferSize;
            return;
        }
        counterBuf.SetData(counterArray);
        ComputeBuffer.CopyCount(deadBuf, counterBuf, 0);
        counterBuf.GetData(counterArray);
        deadCount = counterArray[0];
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

struct Particle
{
    public Vector3 pos, vel;
    public Color col;
    public Vector2 life;
}

struct Vertex2D
{
    public Vector2 pos;
    public Vector2 uv;
}