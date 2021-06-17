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

    private List<GPUParticle> particles = new List<GPUParticle>();
    private float lastSpawnT = 0;

    // Start is called before the first frame update
    void Start()
    {
        // InvokeRepeating("spawnParticle", startDelay, 1 / emissionRate);
    }

    public void spawnParticle()
    {
        Vector3 pos = Vector3.zero; // new Vector3(Random.value, Random.value, Random.value);
        pos.Scale(transform.lossyScale);

        float c = 0.5f + Random.value/2;
        Color col = new Vector4(c, c, c, 1);

        Vector3 vel = 2 * new Vector3(Random.value, Random.value, Random.value) - Vector3.one;
        vel.Normalize();

        particles.Add(new GPUParticle(lifetime, pos, vel, col));
        if (particles.Count > maxParts) particles.RemoveAt(0);
        lastSpawnT = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastSpawnT > 1 / emissionRate) spawnParticle();
        particles.RemoveAll(p => GPUParticle.Update(ref p));
    }

    void OnRenderObject()
    {
        mat.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        particles.ForEach(p => p.Render(this));
        GL.PopMatrix();
    }
}

struct GPUParticle
{
    public Vector3 pos, vel;
    public Color col;
    public float live;

    public GPUParticle(float l, Vector3 p, Vector3 v, Color c)
    {
        pos = p; vel = v; live = l; col = c;
    }

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

    public static bool Update(ref GPUParticle p)
    {
        p.live -= Time.deltaTime;
        if (p.live < 0) return true;

        p.pos += p.vel * Time.deltaTime;
        p.col.a -= 2.5f * Time.deltaTime;
        return false;
    }
}