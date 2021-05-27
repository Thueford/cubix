using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    public float acceleration = 5000f;
    public float maxSpeed = 1000f;

    public float rangle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private Vector3 lastVel = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = Vector2.zero;

        // read input keys
        if (Input.GetKey(KeyCode.DownArrow)) dir.z = -1;
        if (Input.GetKey(KeyCode.RightArrow)) dir.x = 1;
        if (Input.GetKey(KeyCode.UpArrow)) dir.z = 1;
        if (Input.GetKey(KeyCode.LeftArrow)) dir.x = -1;
        dir = dir.normalized * Time.deltaTime;

        // apply direction
        rb.AddForce(acceleration * dir, ForceMode.Acceleration);
        if(rb.velocity.magnitude > maxSpeed) 
            rb.velocity = rb.velocity.normalized * maxSpeed;

        // limit rotation
        /* Vector3 vel = rb.velocity;
        rangle = Vector3.Angle(lastVel, vel);// / Time.deltaTime;
        if (Mathf.Abs(rangle) > 5)
        {
            float s = Mathf.Sign(rangle);
            Vector3.RotateTowards(vel, lastVel, Mathf.Rad2Deg * rangle + s * 5 * Time.deltaTime, 0);
        }
        lastVel = vel; */


        // look in movement direction
        if (rb.velocity.sqrMagnitude != 0) 
            transform.forward = rb.velocity.normalized;
    }
}
