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
    private PlayerShooter bs;

    private Vector3Int rgb = new Vector3Int(0, 0, 0);
    private Vector3Int lastActivatedColor = new Vector3Int(0, 0, 0);

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
    public static void dbgLog(string msg)
    {
        if (self && self.txtDbg) self.txtDbg.text += "\n" + msg;
    }

    // Update is called once per frame
    override public void Update()
    {
        base.Update();
        if (movable)
        {
            Vector3 dir = Vector3.zero;

            rgb = ReadColorInput(rgb);
            // read input keys
            dir = ReadDirInput();
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

            ReadShootInput();
        }
    }

    private Vector3 ReadDirInput()
    {
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) dir.z -= 1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) dir.x += 1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) dir.z += 1;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) dir.x -= 1;
        return dir;
    }

    private Vector3Int ReadColorInput(Vector3Int rgb)
    {
        //Vector3Int rgb = this.rgb;
        if (GameState.self.maxActiveColors == 0) return rgb;

        if (Input.GetKeyDown(KeyCode.Alpha1) && GameState.self.unlockedColors.x == 1)
        {
            rgb.x = 1 - rgb.x;
            if (GameState.self.maxActiveColors == 1 && rgb.x == 1) rgb = Vector3Int.right;
            if (GameState.self.maxActiveColors == 2 && rgb.x + rgb.y + rgb.z == 3)
                rgb = Vector3Int.right + lastActivatedColor;
            if (rgb.x == 1) lastActivatedColor = Vector3Int.right;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && GameState.self.unlockedColors.y == 1)
        {
            rgb.y = 1 - rgb.y;
            if (GameState.self.maxActiveColors == 1 && rgb.y == 1) rgb = Vector3Int.up;
            if (GameState.self.maxActiveColors == 2 && rgb.x + rgb.y + rgb.z == 3)
                rgb = Vector3Int.up + lastActivatedColor;
            if (rgb.y == 1) lastActivatedColor = Vector3Int.up;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && GameState.self.unlockedColors.z == 1)
        {
            rgb.z = 1 - rgb.z;
            if (GameState.self.maxActiveColors == 1 && rgb.z == 1) rgb = Vector3Int.forward;
            if (GameState.self.maxActiveColors == 2 && rgb.x + rgb.y + rgb.z == 3)
                rgb = Vector3Int.forward + lastActivatedColor;
            if (rgb.z == 1) lastActivatedColor = Vector3Int.forward;
        }
        if (rgb != this.rgb)
        {
            bs.updateColor(rgb);
            bs.updateProperties(rgb, this.rgb);
        }
        return rgb;
    }

    private void ReadShootInput()
    {
        if (Input.GetMouseButton(0))
        {
            bs.tryShot();
        }
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
