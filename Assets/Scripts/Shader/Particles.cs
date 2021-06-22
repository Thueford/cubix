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

    [Range(1,(int)1e6)]
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

    [NotNull] public Material mat;
    [NotNull] public Mesh mesh;
    #endregion

    #region Unity
    // Start is called before the first frame update
    void Start()
    {
        mat.enableInstancing = true;
        //InitParticles();
        ShaderSetup();
    }

    // Update is called once per frame
    void Update()
    {
        DispatchUpdate();

        /* update maxParts
        if (curMaxParts != maxParts)
        {
            curMaxParts = maxParts;
            particles = new Particle[maxParts];
        } */


        // spawn particles
        timePerPart = 1 / emissionRate;
        partTimer += Time.deltaTime;
        DispatchEmit((int)(partTimer / timePerPart));
        partTimer -= timePerPart * (int)(partTimer / timePerPart);
        
        // set uniforms
        compute.SetVector("_ParentPosition", transform.position);
        compute.SetFloat("_DeltaTime", Time.deltaTime);

        // mat.SetFloat("_Time", Time.time);

        // render
        compute.Dispatch(kernelUpdate, Mathf.CeilToInt(maxParts / (float)tgx), 1, 1);
        bounds.center = transform.position;
        Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, meshArgsBuf);
    }
    #endregion

    #region Particles
    /************   Particle stuff   ****************/
    /*
    private void InitParticles()
    {
        particles = new Particle[maxParts];
        for (int i = 0; i < maxParts; i++)
        {
            Particle part = new Particle();
            
            part.pos = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
            part.col = Color.Lerp(Color.white, Color.yellow, Random.value);
            part.vel = 2 * new Vector3(Random.value, Random.value, Random.value) - Vector3.one;
            part.vel = 0 * part.vel.normalized;
            part.life = lifetime;

            particles[i] = part;
        }
    }

    public void spawnParticle()
    {
        Vector3 pos = Vector3.zero; // new Vector3(Random.value, Random.value, Random.value);
        pos.Scale(transform.lossyScale);

        float c = 0.5f + Random.value / 2;
        Color col = new Vector4(c, c, c, 1);

        Vector3 vel = 2 * new Vector3(Random.value, Random.value, Random.value) - Vector3.one;
        vel.Normalize();

        uint p = firstUnusedParticle();
        particles[p].init(lifetime, pos, vel, col);
        meshPropertiesBuffer.SetData(particles);
    }

    uint lastUsedParticle = 0;
    uint firstUnusedParticle()
    {
        for (uint i = lastUsedParticle; i < curMaxParts; ++i)
            if (particles[i].life <= 0)
                return lastUsedParticle = i;

        for (uint i = 0; i < lastUsedParticle; ++i)
            if (particles[i].life <= 0)
                return lastUsedParticle = i;

        return 0; // lastUsedParticle = (uint)(Random.value*curMaxParts);
    }
    */
    #endregion

    #region Shader
    /************   Shader stuff   ****************/

    public ComputeShader compute;
    private ComputeBuffer particlesBuf, deadBuf;
    private ComputeBuffer meshArgsBuf;

    private int kernelInit, kernelEmit, kernelUpdate;
    private uint tgx, tgy, tgz;

    #region Shader Setup
    private void ShaderSetup()
    {
        bounds = new Bounds(transform.position, Vector3.one * (range + 1));
        kernelInit= compute.FindKernel("Init");
        kernelEmit = compute.FindKernel("Emit");
        kernelUpdate = compute.FindKernel("Update");
        compute.GetKernelThreadGroupSizes(kernelInit, out tgx, out tgy, out tgz);

        ReleaseBuffers();
        InitializeMeshBuffer();
        InitializePartBuffer();
        compute.Dispatch(kernelInit, Mathf.CeilToInt(maxParts / (float)tgx), 1, 1);
    }

    private void InitializeMeshBuffer()
    {
        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        args[0] = mesh.GetIndexCount(0); // number of triangle indices
        args[1] = (uint)maxParts;        // population
        args[2] = mesh.GetIndexStart(0); // submesh start
        args[3] = mesh.GetBaseVertex(0); // submesh bv
        
        meshArgsBuf = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        meshArgsBuf.SetData(args);
    }

    private void InitializePartBuffer()
    {
        particlesBuf = new ComputeBuffer(maxParts, Marshal.SizeOf(typeof(Particle)));
        compute.SetBuffer(kernelInit, "_Particles", particlesBuf);
        mat.SetBuffer("_Particles", particlesBuf);

        deadBuf = new ComputeBuffer(maxParts, sizeof(int), ComputeBufferType.Append);
        deadBuf.SetCounterValue(0);
        compute.SetBuffer(kernelInit, "_Dead", deadBuf);
    }

    private void ReleaseBuffers()
    {
        if (particlesBuf != null) particlesBuf.Release();
        if (meshArgsBuf != null) meshArgsBuf.Release();
        if (deadBuf != null) deadBuf.Release();
    }

    #endregion

    #region Shader Loop

    public void DispatchEmit(int count)
    {
        if (count <= 0) return;

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
    }

    private void DispatchUpdate()
    {
        compute.SetBuffer(kernelUpdate, "_Particles", particlesBuf);
        compute.SetBuffer(kernelUpdate, "_Dead", deadBuf);

        compute.Dispatch(kernelUpdate, Mathf.CeilToInt(maxParts / (float)tgx), 1, 1);
    }

    #endregion

    private void OnDisable()
    {
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