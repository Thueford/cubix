using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CtxSteer : EntityBase
{
    public static float
        ANGULAR_NOISE = 30,
        F_PERLIN = 0.5f;

    public static Effector_T
        eplayer = new Effector_T("Player", 2, 30, false),
        eenemy = new Effector_T("Enemy", -1, 10),
        ewall = new Effector_T("Wall", -2, 10);

    private static int steersPerSec = 20;
    protected int pnD;
    protected Vector3 steerDir;
    protected float steerdt;

    override public void Awake()
    {
        base.Awake();
        pnD = (int)Random.Range(-1e5f, 1e5f);
    }

    override public void Start()
    {
        base.Start();
        // steersPerSec = (int)(n / Time.fixedDeltaTime);
        steerdt = 1 / (float)steersPerSec;
    }

    private int steerTime;
    override public void FixedUpdate()
    {
        base.FixedUpdate();

        if (movable)
        {
            if(--steerTime <= 0)
            {
                steerTime = Mathf.RoundToInt(1 / (steersPerSec * Time.fixedDeltaTime));
                steerDir = steer();
            }

            // apply direction
            rb.AddForce(steerDir, ForceMode.Acceleration);
        }
    }

    private const int DIRS = 12;
    protected Vector3 contextSteer(List<Vector3> effectors)
    {
        Vector3 dir = Vector3.zero;

        for (int i = 0; i < DIRS; ++i)
        {
            Vector3 a = Quaternion.Euler(0, 360 * i / DIRS, 0) * Vector3.forward;
            float factor = 0;

            // apply effectors
            foreach (Vector3 d in effectors)
                factor += d.magnitude * Vector3.Dot(a, d.normalized);

            // avoid walls
            factor += ewall.getRaycast(gameObject, a);

            // effective dir
            dir += factor * a;
            dbgLine(a, Mathf.Abs(factor), factor > 0 ? Color.green : Color.red);
        }

        if (dir.sqrMagnitude > 1) dir.Normalize();
        return Quaternion.Euler(0, ANGULAR_NOISE * perlinNoise(0.4f * Time.time, pnD), 0) * dir;
    }

    public static float perlinNoise(float time, int seed)
    {
        return 2 * Mathf.PerlinNoise(time, seed/1e3f) - 1;
    }

    public void dbgLine(Vector3 dir, float length, Color color)
    {
        dir.Normalize();
        Debug.DrawLine(transform.position + dir, transform.position + dir * (1 + length), color);
    }

    abstract public Vector3 steer();
}
