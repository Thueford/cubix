using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour
{
    [Header("Particles Properties")]
    [NotNull] public Material mat;
    public float emissionRate = 5;
    public float startDelay = 0;
    public float lifetime = 3;
    [Range(1,(int)1e6)]
    public int maxParts = 1;
    public Vector2 scale = new Vector2(0.2f, 0.2f);
    public Vector3 startSpeed = Vector3.one;

    [Header("Other")]
    //[ReadOnly] public int curParts = 0;
    private float partTimer = 0, timePerPart; // for spawning new particles

    private Particle[] particles;
    private int curMaxParts = 1;

    public Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        mat.enableInstancing = true;
        InitParticles();
        ShaderSetup();
    }

    // Update is called once per frame
    void Update()
    {
        /* update maxParts
        if (curMaxParts != maxParts)
        {
            curMaxParts = maxParts;
            particles = new Particle[maxParts];
        } */

        // spawn particles
        /*
        timePerPart = 1 / emissionRate;
        partTimer += Time.deltaTime;
        for (float t = timePerPart; t < partTimer; t += timePerPart)
            spawnParticle();
        partTimer -= timePerPart * (int)(partTimer / timePerPart);
        */

        // set uniforms
        mat.SetVector("_parentPos", transform.position);
        mat.SetVector("_scale", scale);
        bounds.center = transform.position;

        // render
        compute.Dispatch(kernel, Mathf.CeilToInt(maxParts / (float)tgx), 1, 1);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, argsBuffer);
    }


    /************   Particle stuff   ****************/

    private void InitParticles()
    {
        particles = new Particle[maxParts];
        for (int i = 0; i < maxParts; i++)
        {
            Particle props = new Particle();
            props.pos = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
            props.col = Color.Lerp(Color.white, Color.yellow, Random.value);
            props.init(lifetime, props.pos, Vector3.zero, props.col);

            particles[i] = props;
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


    /************   Shader stuff   ****************/

    public ComputeShader compute;
    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;

    public float range = 5;
    private Bounds bounds;

    private int kernel;
    private uint tgx, tgy, tgz;

    private void ShaderSetup()
    {
        bounds = new Bounds(transform.position, Vector3.one * (range + 1));
        kernel = compute.FindKernel("CSMain");
        compute.GetKernelThreadGroupSizes(kernel, out tgx, out tgy, out tgz);

        InitializeMeshBuffer();
        InitializePartBuffer();
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
        
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    private void InitializePartBuffer()
    {
        meshPropertiesBuffer = new ComputeBuffer(maxParts, Particle.Size());
        meshPropertiesBuffer.SetData(particles);
        compute.SetBuffer(kernel, "_Properties", meshPropertiesBuffer);
        mat.SetBuffer("_Properties", meshPropertiesBuffer);
    }

    private void OnDisable()
    {
        if (meshPropertiesBuffer != null)
            meshPropertiesBuffer.Release();
        meshPropertiesBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }
}

struct Particle
{
    public Vector3 pos, vel;
    public Color col;
    public float life;

    public static int Size()
    {
        return
            sizeof(float) * 3 * 2 + // pos + vel
            sizeof(float) * 4 +     // color
            sizeof(float) * 1;      // life
    }

    public Particle(float l, Vector3 p, Vector3 v, Color c)
    { pos = p; vel = v; life = l; col = c; }

    public void init(float l, Vector3 p, Vector3 v, Color c)
    { pos = p; vel = v; life = l; col = c; }
}