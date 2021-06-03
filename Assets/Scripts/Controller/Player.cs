using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    private const float floatHeight = 0.5f;

    public static Player self;
    public static GameStage curStage;

    private Animator anim;
    private Rigidbody rb;

    public Text txtDbg;
    public GameObject startStage;
    public float accelerationForce = 15f;
    public float maxSpeed = 20f;
    public bool movable = true;

    // Start is called before the first frame update
    void Start()
    {
        self = this;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        if(txtDbg == null) Debug.LogWarning("player.txtDbg not assigned");

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
    void Update()
    {
        if(movable) {
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
            transform.forward = r.GetPoint(f) - transform.position;
        }
    }

    public void TeleportNext()
    {
        if (curStage == null || curStage.next == null) return;
        anim.Play("Teleport");
    }

    void OnTeleport(AnimationEvent ev)
    {
        Teleport(curStage.next.GetComponent<GameStage>());
    }

    public void Teleport(GameStage stage)
    {
        if (stage == null) return;

        if (curStage != null) curStage.OnStageExit();
        stage.OnStageEnter();

        // reset animations
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
    }
}
