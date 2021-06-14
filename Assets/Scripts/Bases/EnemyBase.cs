using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : CtxSteer
{
    [Header("Other Settings")]
    public Color color;
    protected int pnOff;

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();

        if (GameState.self.unlockedColors.x != 0) color.r = 1;
        if (GameState.self.unlockedColors.y != 0) color.g = 1;
        if (GameState.self.unlockedColors.z != 0) color.b = 1;
        GetComponentInChildren<Renderer>().material.color = color;
        GetComponentInChildren<Light>().color = color == GameState.black ? GameState.glow : color;

        pnOff = (int)Random.Range(-1e5f, 1e5f);
    }

    override public void OnSpawn(AnimationEvent ev)
    {
        base.OnSpawn(ev);
        Debug.Log("Enemy Spawn");
        // look at player
        transform.forward = Player.self.transform.position - transform.position;
    }


    public bool isFocused()
    {
        Vector3 dp = Player.self.transform.position - transform.position;
        if (Physics.Raycast(transform.position, dp, out RaycastHit hit, eplayer.dist, LayerMask.GetMask("PlayerPhysics", "Wall")))
        {
            // dbgLine(dp, eplayer.dist, hit.collider.CompareTag("Player") ? Color.magenta : Color.gray);
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    protected Vector3 contextSteer2Player(List<Vector3> effectors)
    {
        bool focused = isFocused();
        if (focused) effectors.Add(eplayer.getEff(gameObject, Player.self.gameObject));
        // eplayer.factor * (Player.self.transform.position - transform.position).normalized);
        dbgLine(eplayer.getEff(gameObject, Player.self.gameObject), eplayer.getEff(gameObject, Player.self.gameObject).magnitude, Color.blue);
        return (focused ? 1 : 0.4f) * contextSteerIDLE(effectors);
    }

    protected float noiseTime = 0;
    protected Vector3 contextSteerIDLE(List<Vector3> effectors)
    {
        // idle noise
        float fnoise = 1;
        if (rb.velocity.magnitude > 0.01) fnoise += 2 / rb.velocity.magnitude;
        noiseTime += 0.4f * steerdt * fnoise;

        Vector3 d = new Vector3(perlinNoise(noiseTime, pnOff), 0, perlinNoise(noiseTime, -pnD));
        effectors.Add(F_PERLIN * d.normalized);
        dbgLine(d, d.magnitude, Color.yellow);
        return contextSteer(effectors);
    }
}
