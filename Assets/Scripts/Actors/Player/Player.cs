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
    [NotNull] public PlayerHP hpDisplay;
    [NotNull] public Light playerLight;
    [NotNull] public PlayerShooter bs;
    [NotNull] public Particles psTrail, psColorSwitch;
    [NotNull] public Animator animFlicker;

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
        setHP(HP);
        if (txtDbg == null) Debug.LogWarning("player.txtDbg not assigned");
        SetShooterColor(rgb);
        //animFlicker.enabled = false;
    }

    public static void dbgSet(string msg) {
        if (self && self.txtDbg) self.txtDbg.text = msg;
    }
    public static void dbgLog(string msg) {
        if (self && self.txtDbg) self.txtDbg.text += "\n" + msg;
    }

    public void SwitchColor()
    {
        Color col = GameState.V2Color(rgb);
        psTrail.color.color = GameState.getLightColor(col);
        psTrail.color.color2 = 0.4f * psTrail.color.color;
        psTrail.color.color2.a = 1;

        psColorSwitch.color.color = psTrail.color.color;
        psColorSwitch.color.color2 = psTrail.color.color2;

        psColorSwitch.ResetPS();
        psColorSwitch.SetEnabled(true);

        playerLight.color = psTrail.color.color;
        hpDisplay.mat.color = col;
    }

    public void SetShooterColor(Vector3Int c)
    {
        bs.updateProperties(rgb = c);
        SwitchColor();
    }

    override public void FixedUpdate()
    {
        base.FixedUpdate();

        if (movable)
        {
            // apply input force
            Vector3 dir = InputHandler.ReadDirInput().normalized;
            rb.AddForce(forceByDrag(maxSpeed, rb.drag) * dir, ForceMode.Acceleration);

            float pvel = Mathf.Clamp(rb.velocity.magnitude / maxSpeed, 1e-2f, 0.96f);
            psTrail.properties.emissionRate = psTrail.properties.maxParts * Mathf.Pow(pvel, 3);
        }
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
                if (col != Vector3.zero) SetShooterColor(col);
            }

            Vector3Int nrgb = InputHandler.ReadColorInput(rgb);
            if (rgb != nrgb) SetShooterColor(nrgb);

            // look in movement direction
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            // intersect mouse ray with floor plane
            float f = (transform.position.y - r.origin.y) / r.direction.y;
            transform.forward = r.GetPoint(f) - transform.position;

            if (InputHandler.ReadShootInput()) bs.tryShot();

        }

        if (invulnurable > 0)
        {
            invulnurable -= Time.deltaTime;
            if (invulnurable <= 0)
            {
                invulnurable = 0;
                animFlicker.Play("Empty");
                rend.enabled = true;
            }
        }
    }

    override public void Hit(float damage)
    {
        if (invulnurable <= 0 && damage > 0)
        {
            MakeInvulnurable(1.25f);

            setHP(HP - damage);

            PostProcessing.self.PlayerHitEffect(0.2f);
        }
    }

    public void setHP(float value)
    {
        HP = Mathf.Max(value, 0);
        hpDisplay.SetHP((int)HP);
        if (HP <= 0) Die();
    }

    public void MakeInvulnurable(float duration)
    {
        if (invulnurable <= 0)
        {
            animFlicker.Play("E_Invuln", 0, 0f);
        }
        invulnurable += duration;
    }

    public void OnCollisionEnter(Collision c)
    {
        if (invulnurable <= 0 && c.collider.CompareTag("Enemy"))
        {
            c.collider.GetComponent<EntityBase>().Die();
            c.collider.GetComponent<SphereCollider>().isTrigger = true;
            Hit(1);
        }
    }

    private GameStage tpTarget;
    public void TeleportNext(GameStage target = null)
    {
        if (target == null) target = GameState.curStage.next;

        if (GameState.curStage.number > GameState.settings.stageHighscore)
        {
            GameState.settings.stageHighscore = GameState.curStage.number;
            GameState.settings.Save();
        }

        if (target == GameState.self.endlessStartStage)
        {
            GameState.settings.reachedEndless = true;
            GameState.settings.Save();
        }

        if (GameState.curStage == null) return;
        else if (target == null)
        {
            target = StageBuilder.self.Generate(GameState.curStage.transform);
        }

        GameState.curStage.FreezeActors();

        tpTarget = target;
        animGeneral.enabled = true;
        animGeneral.Play("Teleport");
        target.Load();

        setHP(Mathf.Min(maxHP, HP+1));
    }


    void OnTeleport(AnimationEvent ev)
    {
        if (tpTarget == null) Debug.LogWarning("Tp Target is null");
        Teleport(tpTarget ? tpTarget : GameState.curStage.next);
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

        animGeneral.enabled = true;
        animGeneral.Play("Spawn");

        Vector3 spawnPos = stage.spawn.transform.position;
        spawnPos.y = floatHeight;
        self.transform.position = spawnPos;

        GameState.curStage = stage;
        stage.OnStageEnter();
        if (GameState.stateCurStage.stage != stage) GameState.SaveCurState();
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
