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
    [NotNull] public PlayerShooter bs;
    [NotNull] public Particles psTrail, psColorSwitch;

    private float invulnurable = 0;

    override public void Awake()
    {
        base.Awake();
        self = this;
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

    public void SwitchColor()
    {
        psTrail.color.color = GameState.getLightColor(GameState.V2Color(rgb));
        psTrail.color.color2 = 0.4f * psTrail.color.color;
        psTrail.color.color2.a = 1;

        psColorSwitch.color.color = psTrail.color.color;
        psColorSwitch.color.color2 = psTrail.color.color2;

        psColorSwitch.ResetPS();
        psColorSwitch.SetEnabled(true);
    }

    public void SetShooterColor(Vector3Int c)
    {
        bs.updateProperties(rgb = c);
        SwitchColor();
    }

    // Update is called once per frame
    override public void Update()
    {
        base.Update();

        if (movable)
        {
            if (InputHandler.ReadSpaceInput())
            {
                Vector3Int col = Ressource.activateColors();
                if(col != Vector3.zero) SetShooterColor(col);
            }
            
            Vector3Int nrgb = InputHandler.ReadColorInput(rgb);
            if (rgb != nrgb) SetShooterColor(nrgb);

            // read input keys
            Vector3 dir = InputHandler.ReadDirInput();
            dir = dir.normalized * accelerationForce;

            // apply direction
            //float vel = rb.velocity.magnitude;
            rb.AddForce(Time.deltaTime * 1000 * dir, ForceMode.Acceleration);
            if(rb.velocity.magnitude > maxSpeed)
            {
                //if (rb.velocity.magnitude > vel) rb.velocity = rb.velocity.normalized * vel;
                rb.velocity = rb.velocity.normalized * maxSpeed;
                //rb.AddForce(-Time.deltaTime * 2000 * (rb.velocity.normalized), ForceMode.Acceleration);
            }

            float pvel = Mathf.Clamp(rb.velocity.magnitude / maxSpeed, 1e-2f, 0.96f);
            psTrail.properties.emissionRate = psTrail.properties.maxParts * Mathf.Pow(pvel, 3);

            // look in movement direction
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            // intersect mouse ray with floor plane
            float f = (transform.position.y-r.origin.y)/r.direction.y;
            transform.forward = r.GetPoint(f) - transform.position;

            if(InputHandler.ReadShootInput()) bs.tryShot();

            if (invulnurable > 0)
            {
                Debug.Log("Play Flicker Animation");
                invulnurable = Mathf.Max(0, invulnurable - Time.deltaTime);
                if (invulnurable == 0) Debug.Log("Stop Flicker Animation");
            }
        }
    }

    override public void Hit(float damage)
    {
        if (invulnurable <= 0 && damage > 0) 
        {
            HP -= damage;
            invulnurable = 1;
            PostProcessing.self.PlayerHitEffect(0.2f);
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
        if (GameState.curStage == null) return;
        else if (GameState.curStage.next == null)
            GameState.curStage.next = StageBuilder.self.Generate(GameState.curStage.transform);

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
        if (GameState.curStage != null && GameState.curStage != stage)
        {
            GameState.curStage.Unload();
            if (GameState.curStage.isProcedural)
            {
                Destroy(GameState.curStage.gameObject);
                Vector3 camPos = Camera.main.transform.position;
                camPos.z -= 40;
                Camera.main.transform.position = camPos;
            }
        }

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

    public override void OnDie(AnimationEvent ev)
    {
        base.OnDie(ev);
        GameState.load(GameState.stateCurStage);
    }
}
