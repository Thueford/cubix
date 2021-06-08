using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class Player : EntityBase
{
    private const float floatHeight = 0.5f;

    public static Player self;
    public static GameStage curStage;
    public float startHP;

    public Text txtDbg;

    public PlayerShooter bs;
    private Vector3Int rgb = new Vector3Int(0, 0, 0);

    // Start is called before the first frame update
    override protected void Start()
    {
        HP = startHP;
        base.Start();
        self = this;
        bs = gameObject.GetComponent<PlayerShooter>();
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
    override protected void Update()
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
            Ray r = FindObjectOfType<GameCamera>().GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            rgb.x = 1 - rgb.x;
            bs.toggleRed(rgb.x == 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            rgb.y = 1 - rgb.y;
            bs.toggleGreen(rgb.y == 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            rgb.z = 1 - rgb.z;
            bs.toggleBlue(rgb.z == 1);
        }
        if (rgb != this.rgb)
        {
            bs.updateColor(rgb);
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

        curStage.next.GetComponent<GameStage>().OnStageEntering();
    }

    void OnTeleport(AnimationEvent ev)
    {
        Teleport(curStage.next.GetComponent<GameStage>());
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
