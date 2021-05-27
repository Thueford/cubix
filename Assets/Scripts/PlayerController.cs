using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    public float acceleration = 10f;
    public float maxSpeed = 1000f;

    public float rangle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private Vector3 lastDir = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = Vector2.zero;

        // read input keys
        if (Input.GetKey(KeyCode.DownArrow)) dir.z = -1;
        if (Input.GetKey(KeyCode.RightArrow)) dir.x = 1;
        if (Input.GetKey(KeyCode.UpArrow)) dir.z = 1;
        if (Input.GetKey(KeyCode.LeftArrow)) dir.x = -1;
        dir.Normalize();

        // limit rotation
        /* rangle = Vector3.Angle(lastDir, dir) / Time.deltaTime;
        if (Mathf.Abs(rangle) > 30)
        {
            float s = Mathf.Sign(rangle);
            Vector3.RotateTowards(dir, lastDir, Mathf.Rad2Deg * rangle+s*30*Time.deltaTime, 0);
        } */

        lastDir = dir;

        // apply direction
        rb.AddForce(acceleration * dir, ForceMode.Acceleration);
        if(rb.velocity.magnitude > maxSpeed) 
            rb.velocity = rb.velocity.normalized * maxSpeed;

        // look in movement direction
        if (rb.velocity.sqrMagnitude != 0) 
            transform.forward = rb.velocity.normalized;
    }
}
