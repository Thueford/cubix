using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerShooter))]
public class Player : EntityBase
{
    private const float floatHeight = 0.5f;

    public static Player self;

    [Header("Other Settings")]
    [NotNull, HideInInspector] public PlayerHP hpDisplay;
    [NotNull, HideInInspector] public PlayerShooter bs;
    [NotNull, HideInInspector] public Particles psTrail, psColorSwitch;
    [NotNull, HideInInspector] public Animator animFlicker;

    private float invulnurable = 0;
    private GameStage tpTarget;

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
        SetShooterColor(rgb);
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

        vlight.color = psTrail.color.color;
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

            if (InputHandler.ReadShootInput()) bs.tryShot();
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
            float f = (pos.y - r.origin.y) / r.direction.y;
            transform.forward = r.GetPoint(f) - pos;
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
        else if (damage < 0)
        {
            setHP(HP - damage);
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
            GameState.playerStats.sacrifices++;
            c.collider.GetComponent<EntityBase>().Die();
            c.collider.GetComponent<SphereCollider>().isTrigger = true;
            Hit(1);
        }
    }

    public void TeleportNext(GameStage target = null)
    {
        if (GameState.curStage == null) return;
        if (target == null) target = GameState.curStage.next;
        if (target == null) target = StageBuilder.self.Generate(GameState.curStage.transform);

        GameState.updateClearStats(target);
        if (GameState.curStage > 0) setHP(Mathf.Min(maxHP, HP + 1));
        Teleport(target);
    }

    public void Teleport(GameStage target)
    {
        GameState.curStage.FreezeActors();

        tpTarget = target;
        animGeneral.enabled = true;
        animGeneral.Play("Teleport");
    }


    void OnTeleport(AnimationEvent ev)
    {
        if (tpTarget == null)
        {
            Debug.LogWarning("Tp Target is null");
            tpTarget = GameState.curStage.next;
        }
        SpawnAt(tpTarget);
    }

    public void SpawnAt(GameStage stage)
    {
        if (stage == null) return;
        Freeze();
        GameState.SwitchStage(stage);

        animGeneral.enabled = true;
        animGeneral.Play("Spawn", 0, 0);
        vlight.enabled = true;

        Vector3 spawnPos = stage.spawn.transform.position;
        spawnPos.y = floatHeight;
        transform.position = spawnPos;
    }

    override public void OnSpawn(AnimationEvent ev)
    {
        base.OnSpawn(ev);
        GameState.curStage.MeltActors();
    }

    public override void Die()
    {
        base.Die();
        GameState.curStage.FreezeActors();
        GameState.updateDeadStats();
    }

    public override void OnDie(AnimationEvent ev)
    {
        base.OnDie(ev);
        GameState.RestartStage();
    }
}
