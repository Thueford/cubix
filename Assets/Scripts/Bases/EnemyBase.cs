using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : EntityBase
{
    public const float
        ANGULAR_NOISE = 40,
        MAXDIST_E = 10,
        MAXDIST_W = 10,

        F_WALLS = -2f,
        F_PLAYER = 2,
        F_ENEMIES = -1;

    public Color color;

    private int pnD;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        
        pnD = Random.Range(int.MinValue, int.MaxValue);

        if (GameState.self.unlockedColors.x != 0) color.r = 1;
        if (GameState.self.unlockedColors.y != 0) color.g = 1;
        if (GameState.self.unlockedColors.z != 0) color.b = 1;
        GetComponentInChildren<Renderer>().material.color = color;
        GetComponentInChildren<Light>().color = color == GameState.black ? GameState.glow : color;
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
    }

    private const int DIRS = 12;
    protected Vector3 contextSteer(Vector3[] effectors, float fwalls, float maxdistW)
    {
        Vector3 dir = Vector3.zero;

        for (int i = 0; i < DIRS; ++i)
        {
            Vector3 a = Quaternion.Euler(0, 360 * i / DIRS, 0) * Vector3.forward;
            float factor = 0;

            foreach (Vector3 d in effectors)
                factor += d.magnitude * Vector3.Dot(a, d.normalized);

            // avoid walls
            if (Physics.Raycast(transform.position, a, out RaycastHit hitInfo, maxdistW/*, layerMask: 1<<9*/))
                if (hitInfo.collider.CompareTag("Wall"))
                    factor += fwalls * (1 - hitInfo.distance / maxdistW);

            // effective dir
            dir += factor * a;
            dbgLine(a, Mathf.Abs(factor), factor > 0 ? Color.green : Color.red);
        }

        return Quaternion.Euler(0, ANGULAR_NOISE * perlinNoise(0.4f, pnD), 0) * dir;
    }

    protected Vector3 avoid(EntityBase e, float maxdist)
    {
        Vector3 d = e.transform.position - transform.position;
        d.y = 0;
        if (d.magnitude >= maxdist) d = Vector3.zero;
        else d = -(1 - d.magnitude / maxdist) * d.normalized;
        return d;
    }

    override public void OnSpawn(AnimationEvent ev)
    {
        base.OnSpawn(ev);
        Debug.Log("Enemy Spawn");
        // look at player
        transform.forward = Player.self.transform.position - transform.position;
    }

    public void dbgLine(Vector3 dir, float length, Color color)
    {
        dir.Normalize();
        Debug.DrawLine(transform.position + dir, transform.position + dir * (1 + length), color);
    }

    public static float perlinNoise(float tscale, int seed)
    {
        return 2 * Mathf.PerlinNoise(Time.time*tscale+seed%10, seed) - 1;
    }

    public static float sinNoise(float tscale, int seed)
    {
        return 2 * Mathf.PerlinNoise(Time.time * tscale + seed % 10, seed) - 1;
    }
}
