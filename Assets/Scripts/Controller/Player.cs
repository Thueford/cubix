using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    public static Player self;
    public static GameStage curStage;

    private Rigidbody rb;
    public Text txtDbg;
    public float acceleration = 15f;
    public float maxSpeed = 20f;

    // Start is called before the first frame update
    void Start()
    {
        self = this;
        rb = GetComponent<Rigidbody>();
        if(txtDbg == null) Debug.LogWarning("player.txtDbg not assigned");

        // spawn in stage0
        GameStage[] stages = FindObjectsOfType<GameStage>();
        Debug.Assert(stages.Length != 0, "no stages found");
        Teleport(stages[stages.Length-1]);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = Vector2.zero;

        // read input keys
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) dir.z = -1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) dir.x = 1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) dir.z = 1;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) dir.x = -1;
        dir = dir.normalized * acceleration;

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

    public static void TeleportNext()
    {
        if (curStage == null || curStage.next == null) return;
        Teleport(curStage.next.GetComponent<GameStage>());
    }

    public static void Teleport(GameStage stage)
    {
        if (stage == null) return;
        Debug.Log("Stage: " + stage.gameObject.name);

        // reset animations
        if (curStage != null) curStage.GetComponentInChildren<Portal>().Disable();
        // stage.portal.GetComponent<Portal>().Disable();

        curStage = stage;
        curStage.GetComponentInChildren<ChargeAnim>().Reset();

        // copy camera
        GameCamera cam = FindObjectOfType<GameCamera>();
        cam.target = stage.cam.transform.position;
        cam.transform.rotation = stage.cam.transform.rotation;
        self.transform.position = stage.spawn.transform.position;
    }
}
