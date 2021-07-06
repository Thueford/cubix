using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : CtxSteer
{
    [Header("Other Settings")]
    public Color color;
    protected int pnOff;

    public static bool ctxIDLE = true;
    public const float resDrop = 10;

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        animGeneral.Play("Spawn");
        pnOff = (int)Random.Range(-1e5f, 1e5f);
    }

    override public void OnSpawn(AnimationEvent ev)
    {
        base.OnSpawn(ev);
        transform.forward = Player.self.transform.position - transform.position;
        EnemySpawner.EnemySpawned(rgb.z == 1);
    }

    public void setColor(Color c)
    {
        _color = c;
        rgb = Vector3Int.FloorToInt((Vector4)c);
        rend.material.color = c;
        vlight.color = GameState.getLightColor(c);

        if (c.r == 1) { maxSpeed -= 1; HP = startHP *= 2f; }
        if (c.g == 1) maxSpeed += 2;
        if (c.b == 1) HP = startHP *= 0.5f;
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
        if (focused) effectors.Add(eplayer.getEff(this, Player.self));
        // eplayer.factor * (Player.self.transform.position - transform.position).normalized);
        
        return (focused ? 1 : 0.4f) * contextSteerIDLE(effectors);
    }

    protected float noiseTime = 0;
    protected Vector3 contextSteerIDLE(List<Vector3> effectors)
    {
        // idle noise
        float fnoise = 1;
        if (rb.velocity.magnitude > 0.01) fnoise += 2 / rb.velocity.magnitude;
        noiseTime += 0.4f * steerdt * fnoise;

        if(ctxIDLE) {
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
        EnemySpawner.EnemyDied(rgb.z == 1);
        float res = rgb.z == 1 ? resDrop / 2 : resDrop;
        if (rgb.x == 1) Ressource.self.addRes(Ressource.col.Red, res);
        if (rgb.y == 1) Ressource.self.addRes(Ressource.col.Green, res);
        if (rgb.z == 1) Ressource.self.addRes(Ressource.col.Blue, res);
    }

    public override void OnDie(AnimationEvent ev)
    {
        base.OnDie(ev);
        Collectable.drop(rgb, transform.position);
        Destroy(gameObject);
    }
}
