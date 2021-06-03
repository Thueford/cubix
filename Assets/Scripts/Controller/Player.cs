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
    public GameObject startStage;

    // Start is called before the first frame update
    override protected void Start()
    {
        HP = startHP;
        base.Start();
        self = this;
        if(txtDbg == null) Debug.LogWarning("player.txtDbg not assigned");

        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.2f);

        // spawn in stage0
        if (startStage == null)
        {
            GameStage[] stages = FindObjectsOfType<GameStage>();
            Debug.Assert(stages.Length != 0, "no stages found");
            Teleport(stages[0]);
        }
        else Teleport(startStage.GetComponent<GameStage>());
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
        if (movable)
        {
            Vector3 dir = Vector2.zero;

            // read input keys
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) dir.z = -1;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) dir.x = 1;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) dir.z = 1;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) dir.x = -1;
            dir = dir.normalized * accelerationForce;

            // apply direction
            rb.AddForce(dir*Time.deltaTime*1000, ForceMode.Acceleration);
            if(rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;

            // look in movement direction
            Ray r = FindObjectOfType<GameCamera>().GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            // intersect mouse ray with floor plane
            float f = (transform.position.y-r.origin.y)/r.direction.y;
            txtDbg.text = (r.GetPoint(f) - transform.position).ToString();
            transform.forward = r.GetPoint(f) - transform.position;
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

        // copy camera
        GameCamera cam = FindObjectOfType<GameCamera>();
        cam.target = stage.cam.transform.position;
        cam.transform.rotation = stage.cam.transform.rotation;
        self.transform.position = stage.spawn.transform.position;

        curStage = stage;
        Vector3 spawnPos = stage.spawn.transform.position;
        spawnPos.y = floatHeight;
        self.transform.position = spawnPos;

        stage.OnStageEnter();
    }

    public void OnSpawn(AnimationEvent ev)
    {
        anim.enabled = false;
        curStage.OnStageEntered();
    }
}
