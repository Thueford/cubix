using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    public Text txtDbg;
    public float acceleration = 15f;
    public float maxSpeed = 20f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Assert.IsNotNull(txtDbg, "player.txtDbg not assigned");
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
        rb.AddForce(dir, ForceMode.Acceleration);
        if(rb.velocity.magnitude > maxSpeed) 
            rb.velocity = rb.velocity.normalized * maxSpeed;


        // look in movement direction
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        // intersect mouse ray with floor plane
        float f = (transform.position.y-r.origin.y)/r.direction.y;
        transform.forward = r.GetPoint(f) - transform.position;
    }
}
