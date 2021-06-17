using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GPUParticles : MonoBehaviour
{
    [NotNull] public Material mat;
    public float emissionRate = 5;
    public float startDelay = 0;
    public float lifetime = 3;
    public int maxParts = 1;
    public Vector2 scale = new Vector2(0.2f, 0.2f);
    public Vector3 startSpeed = Vector3.one;
    public AnimationCurve curve;

    private GPUParticle[] particles;
    private float lastSpawnT = 0;
    private int curMaxParts = 1;

    // Start is called before the first frame update
    void Start()
    {
        particles = new GPUParticle[maxParts];
        // InvokeRepeating("spawnParticle", startDelay, 1 / emissionRate);
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

        return lastUsedParticle = 0;
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
        lastSpawnT = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(curMaxParts != maxParts)
            particles = new GPUParticle[curMaxParts = maxParts];

        if (Time.time - lastSpawnT > 1 / emissionRate) spawnParticle();
        // particles.RemoveAll(p => GPUParticle.Update(ref p));
        for (int i = 0; i < maxParts; i++)
            particles[i].Update();
    }

    void OnRenderObject()
    {
        mat.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        foreach (var p in particles) p.Render(this);
        GL.PopMatrix();
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
        GL.PushMatrix();
        // Matrix4x4 look = Matrix4x4.LookAt(pos, Camera.main.transform.position, Vector3.up)
        GL.MultMatrix(Matrix4x4.Translate(p.transform.position + pos) * Matrix4x4.Scale(p.scale));
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
        GL.PopMatrix();
    }

    public bool Update()
    {
        life -= Time.deltaTime;
        if (life < 0) return true;

        pos += vel * Time.deltaTime;
        col.a -= 2.5f * Time.deltaTime;
        return false;
    }
}