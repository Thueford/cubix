using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : CtxSteer
{
    [Header("Other Settings")]
    protected int pnOff;

    public static bool ctxIDLE = true;
    public const float resDrop = 10;

    override public void Awake()
    {
        base.Awake();
        setColor(_color);
        animGeneral.Play("Spawn", 0, 0);
        pnOff = (int)Random.Range(-1e5f, 1e5f);
    }

    public override void Start()
    {
        base.Start();
        EnemySpawner.EnemySpawned(this);
    }
    public static int getColorCount(Color c) => c.b == 1 || c == Color.white ? 2 : 1;
    public float countWeight => 1 / getColorCount(_color);

    override public void OnSpawn(AnimationEvent ev)
    {
        base.OnSpawn(ev);
        transform.forward = Player.self.pos - pos;
    }

    public void setColor(Color c)
    {
        _color = c.a == 0 ? GameState.black : c;
        rgb = Vector3Int.FloorToInt((Vector4)c);
        rend.material.color = c;
        vlight.color = GameState.getLightColor(c);

        if (c == Color.white)
        {
            maxSpeed += 2;
            HP = startHP *= 5 / 3f;
        }
        else
        {
            if (c.r == 1) { maxSpeed -= 1; HP = startHP *= 2f; }
            if (c.g == 1) maxSpeed += 2;
            if (c.b == 1) HP = startHP *= 0.5f;
        }
    }


    public bool isFocused()
    {
        Vector3 dp = Player.self.pos - pos;
        if (Physics.Raycast(pos, dp, out RaycastHit hit, eplayer.dist, LayerMask.GetMask("PlayerPhysics", "Wall")))
        {
            // dbgLine(dp, eplayer.dist, hit.collider.CompareTag("Player") ? Color.magenta : Color.gray);
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    protected Vector3 contextSteer2Player(List<Vector3> effectors)
    {
        bool focused = isFocused();
        if (focused) effectors.Add(eplayer.getEff(this, Player.self));
        // eplayer.factor * (Player.self.pos - pos).normalized);

        return (focused ? 1 : 0.4f) * contextSteerIDLE(effectors);
    }

    protected float noiseTime = 0;
    protected Vector3 contextSteerIDLE(List<Vector3> effectors)
    {
        // idle noise
        float fnoise = 1;
        if (rb.velocity.magnitude > 0.01) fnoise += 2 / rb.velocity.magnitude;
        noiseTime += 0.4f * steerdt * fnoise;

        if(ctxIDLE)
        {
            Vector3 d = new Vector3(perlinNoise(noiseTime, pnOff), 0, perlinNoise(noiseTime, -pnD));
            effectors.Add(F_PERLIN * d.normalized);
        }

        // avoid enemies
        // EnemyBase[] enemies = GameState.curStage.GetComponentsInChildren<EnemyBase>();
        effectors.AddRange(eenemy.getEffs(this));
        return contextSteer(effectors);
    }

    public override void Die()
    {
        base.Die();
        tag = "Untagged";
        EnemySpawner.EnemyDied(this);

        float res = rgb.z == 1 ? resDrop / 2 : resDrop;
        if (rgb.sqrMagnitude > 0)
            ResParts.Spawn(pos, (int)res, GameState.V2Color(rgb));

        if (rgb.x == 1) Ressource.self.addRes(Ressource.col.Red, res);
        if (rgb.y == 1) Ressource.self.addRes(Ressource.col.Green, res);
        if (rgb.z == 1) Ressource.self.addRes(Ressource.col.Blue, res);
    }

    public override void OnDie(AnimationEvent ev)
    {
        base.OnDie(ev);
        GameState.save.stats.totalKills++;
        Collectable.Drop(rgb, pos);
        Destroy(gameObject);
    }

    override public void Freeze()
    {
        base.Freeze();
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    override public void Melt()
    {
        base.Melt();
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = true;
    }
}
