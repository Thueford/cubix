using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerShooter))]
public class Player : EntityBase
{
    private const float floatHeight = 0.5f;

    public static Player self;

    [Header("Other Settings")]
    [WarnNull] public Text txtDbg;
    public PlayerShooter bs;
    public Particles ps;

    private float invulnurable = 0;

    override public void Awake()
    {
        base.Awake();
        self = this;
        bs = GetComponent<PlayerShooter>();
        ps = GetComponentInChildren<Particles>();
    }

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        HP = startHP;
        if (txtDbg == null) Debug.LogWarning("player.txtDbg not assigned");
    }

    public static void dbgSet(string msg) {
        if (self && self.txtDbg) self.txtDbg.text = msg;
    }
    public static void dbgLog(string msg) {
        if (self && self.txtDbg) self.txtDbg.text += "\n" + msg;
    }

    // Update is called once per frame
    override public void Update()
    {
        base.Update();

        if (movable)
        {
            Vector3 dir = Vector3.zero;

            Vector3Int nrgb = InputHandler.ReadColorInput(rgb);
            if(rgb != nrgb) bs.updateProperties(rgb = nrgb);

            // read input keys
            dir = InputHandler.ReadDirInput();
            dir = dir.normalized * accelerationForce;

            // apply direction
            rb.AddForce(dir*Time.deltaTime*1000, ForceMode.Acceleration);
            if(rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;

            float pvel = Mathf.Clamp(rb.velocity.magnitude / maxSpeed, 1e-2f, 0.96f);
            ps.properties.emissionRate = ps.properties.maxParts * Mathf.Pow(pvel, 3);

            // look in movement direction
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            // intersect mouse ray with floor plane
            float f = (transform.position.y-r.origin.y)/r.direction.y;
            transform.forward = r.GetPoint(f) - transform.position;

            if(InputHandler.ReadShootInput()) bs.tryShot();

            invulnurable = Mathf.Max(0, invulnurable-Time.deltaTime);
        }
    }

    override public void Hit(float damage)
    {
        if (invulnurable <= 0 && damage > 0) 
        { 
            HP -= damage;
            invulnurable = 1;
        }
        if (HP <= 0) Die();
    }

    public void setHP(float value)
    {
        HP = value;
        if (HP <= 0) Die();
    }

    public void OnCollisionEnter(Collision c)
    {
        if (c.collider.CompareTag("Enemy"))
        {
            c.collider.GetComponent<EntityBase>().Die();
            c.collider.GetComponent<SphereCollider>().isTrigger = true;
            Hit(1);
        }
    }

    public void TeleportNext()
    {
        if (GameState.curStage == null || GameState.curStage.next == null) return;

        GameState.curStage.FreezeActors();

        anim.enabled = true;
        anim.Play("Teleport");

        GameState.curStage.next.Load();

        HP = Mathf.Min(maxHP, HP+1);
    }


    void OnTeleport(AnimationEvent ev)
    {
        Teleport(GameState.curStage.next);
    }

    public void Teleport(GameStage stage)
    {
        if (stage == null) return;
        if (GameState.curStage != null && GameState.curStage != stage) GameState.curStage.Unload();

        anim.enabled = true;
        anim.Play("Spawn");

        Vector3 spawnPos = stage.spawn.transform.position;
        spawnPos.y = floatHeight;
        self.transform.position = spawnPos;

        GameState.curStage = stage;
        stage.OnStageEnter();
        GameState.SaveCurState();
    }

    override public void OnSpawn(AnimationEvent ev)
    {
        base.OnSpawn(ev);
        Debug.Log("Player Spawn");
        GameState.curStage.MeltActors();
    }

    public override void Die()
    {
        base.Die();
        GameState.curStage.FreezeActors();
    }
}
