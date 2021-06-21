using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUParticles : MonoBehaviour
{
    [Header("ParticleSystem Properties")]
    [NotNull] public Material mat;
    public float emissionRate = 5;
    public float startDelay = 0;
    public float lifetime = 3;
    [Range(1,(int)1e6)]
    public int maxParts = 1;
    public Vector2 scale = new Vector2(0.2f, 0.2f);
    public Vector3 startSpeed = Vector3.one;

    [Header("Other")]
    [ReadOnly] public int curParts = 0;
    private float timePerPart, partTimer = 0;

    private GPUParticle[] particles;
    private Matrix4x4[] transforms;
    private int curMaxParts = 1;

    // public MeshFilter mf;
    public Mesh mesh;

    // private float partsPerFrame, partTimer;

    // Start is called before the first frame update
    void Start()
    {
        mat.enableInstancing = true;
        particles = new GPUParticle[maxParts];
        transforms = new Matrix4x4[maxParts];
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

    public void spawnParticle()
    {
        Vector3 pos = Vector3.zero; // new Vector3(Random.value, Random.value, Random.value);
        pos.Scale(transform.lossyScale);

        float c = 0.5f + Random.value/2;
        Color col = new Vector4(c, c, c, 1);

        Vector3 vel = 2 * new Vector3(Random.value, Random.value, Random.value) - Vector3.one;
        vel.Normalize();

        uint p = firstUnusedParticle();
        particles[p].init(lifetime, pos, vel, col);
    }

    // Update is called once per frame
    void Update()
    {
        timePerPart = 1/emissionRate;

        mat.SetVector("_parentPos", transform.position);
        mat.SetVector("_scale", scale);

        // update maxParts
        if (curMaxParts != maxParts)
        {
            curMaxParts = maxParts;
            particles = new GPUParticle[maxParts];
            transforms = new Matrix4x4[maxParts];
        }

        // spawn particles
        partTimer += Time.deltaTime;
        for (float t = timePerPart; t < partTimer; t += timePerPart)
            spawnParticle();
        partTimer -= timePerPart * (int)(partTimer / timePerPart);
        
        // update particles
        curParts = 0;
        for (int i = 0; i < maxParts; i++) {
            particles[i].Update();
            transforms[i] = particles[i].getTransform(this);
            // Matrix4x4.Translate(particles[i].pos);
            if (particles[i].life > 0) curParts++;
            if (particles[i].life > 0) {
                // Debug.DrawLine(transform.position, transform.position + particles[i].pos);
                //Graphics.DrawMesh(mesh, transform.position + particles[i].pos, 
                //    Quaternion.identity, mat, 0);
                Graphics.DrawMesh(mesh, transforms[i], mat, 0);
            }
        }

        //Graphics.DrawMeshInstanced(mesh, 0, mat, transforms, 10);
    }

    void OnRenderObject()
    {
        //mat.SetPass(0);
        //GL.PushMatrix();
        //GL.MultMatrix(transform.localToWorldMatrix);
        //foreach (GPUParticle p in particles) p.Render(this);
        //GL.PopMatrix();

        //Graphics.DrawMeshInstanced(mf.mesh, 0, mat, transforms, curMaxParts, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, Camera.main, UnityEngine.Rendering.LightProbeUsage.Off);
    }
}

struct GPUParticle
{
    public Vector3 pos, vel;
    public Color col;
    public float life;

    public GPUParticle(float l, Vector3 p, Vector3 v, Color c)
    { pos = p; vel = v; life = l; col = c; }

    public void init(float l, Vector3 p, Vector3 v, Color c)
    { pos = p; vel = v; life = l; col = c; }

    public void Render(GPUParticles p)
    {
        if (life <= 0) return;
        //GL.PushMatrix();
        // Matrix4x4 look = Matrix4x4.LookAt(pos, Camera.main.transform.position, Vector3.up)
        //GL.MultMatrix(Matrix4x4.Translate(pos));

        GL.Begin(GL.QUADS);
        GL.Color(col);
        GL.TexCoord(Vector3.zero);
        GL.Vertex3(0, 0, 0);
        GL.TexCoord(Vector3.up);
        GL.Vertex3(0, 1, 0);
        GL.TexCoord(new Vector3(1, 1, 0));
        GL.Vertex3(1, 1, 0);
        GL.TexCoord(Vector3.right);
        GL.Vertex3(1, 0, 0);
        GL.End();
        //GL.PopMatrix();
    }

    public void Update()
    {
        if (life < 0) return;
        life -= Time.deltaTime;

        pos += vel * Time.deltaTime;
        col.a -= 2.5f * Time.deltaTime;
    }

    internal Matrix4x4 getTransform(GPUParticles p)
    {
        return Matrix4x4.Translate(p.transform.position + pos);
    }
}