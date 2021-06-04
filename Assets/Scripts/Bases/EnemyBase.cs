using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : EntityBase
{

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
    }

    protected Vector3 avoid(EntityBase e, float maxdist)
    {
        Vector3 d = e.transform.position - transform.position;
        d.y = 0;
        if (d.magnitude >= maxdist) d = Vector3.zero;
        else d = -(1 - d.magnitude / maxdist) * d.normalized;
        return d;
    }

    public void OnSpawn(AnimationEvent ev)
    {
        anim.enabled = false;
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
}
