using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    public Text txtDbg;
    public Camera cam;
    public float acceleration = 5000f;
    public float maxSpeed = 1000f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        dir = dir.normalized * Time.deltaTime;

        // apply direction
        rb.AddForce(acceleration * dir, ForceMode.Acceleration);
        if(rb.velocity.magnitude > maxSpeed) 
            rb.velocity = rb.velocity.normalized * maxSpeed;

        // TODO: look in movement direction
        txtDbg.text = cam.ScreenToWorldPoint(Input.mousePosition).ToString();
        transform.forward = cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
    }
}
