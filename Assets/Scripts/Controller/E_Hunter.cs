using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Hunter : EnemyBase
{
    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    override public void Update()
    {
        base.Update();
        if (movable)
        {
            Vector3 dir = Player.self.transform.position - transform.position;
            dir = dir.normalized * accelerationForce;

            // apply direction
            rb.AddForce(dir * Time.deltaTime * 1000, ForceMode.Acceleration);
            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;

            // look in movement direction
            transform.forward = dir;
        }
    }
}
