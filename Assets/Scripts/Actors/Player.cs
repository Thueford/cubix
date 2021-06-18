using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerShooter))]
public class Player : EntityBase
{
    private const float floatHeight = 0.5f;

    public static Player self;
    public static GameStage curStage;

    [Header("Other Settings")]
    [WarnNull] public Text txtDbg;
    public PlayerShooter bs;

    private float invulnurable = 0;

    override public void Awake()
    {
        self = this;
    }

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        HP = startHP;
        bs = GetComponent<PlayerShooter>();
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
        InputHandler.ReadPauseInput();

        if (movable)
        {
            Vector3 dir = Vector3.zero;

            rgb = InputHandler.ReadColorInput(rgb);
            // read input keys
            dir = InputHandler.ReadDirInput();
            dir = dir.normalized * accelerationForce;

            // apply direction
            rb.AddForce(dir*Time.deltaTime*1000, ForceMode.Acceleration);
            if(rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;

            // look in movement direction
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            // intersect mouse ray with floor plane
            float f = (transform.position.y-r.origin.y)/r.direction.y;
            transform.forward = r.GetPoint(f) - transform.position;

            InputHandler.ReadShootInput();

            invulnurable = Mathf.Max(0, invulnurable-Time.deltaTime);
        }
    }

    override public void Hit(float damage)
    {
        if (invulnurable <= 0) 
        { 
            HP -= damage;
            invulnurable = 1;
        }
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
        if (curStage == null || curStage.next == null) return;

        curStage.OnStageExit();

        anim.enabled = true;
        anim.Play("Teleport");

        curStage.next.OnStageEntering();

        HP = Mathf.Min(maxHP, HP+1);
    }

    void OnTeleport(AnimationEvent ev)
    {
        Teleport(curStage.next);
    }

    public void Teleport(GameStage stage)
    {
        if (stage == null) return;
        if (curStage != null) curStage.OnStageExited();

        anim.enabled = true;
        anim.Play("Spawn");

        Vector3 spawnPos = stage.spawn.transform.position;
        spawnPos.y = floatHeight;
        self.transform.position = spawnPos;

        curStage = stage;
        stage.OnStageEnter();
    }

    override public void OnSpawn(AnimationEvent ev)
    {
        base.OnSpawn(ev);
        Debug.Log("Player Spawn");
        curStage.OnStageEntered();
    }
}
