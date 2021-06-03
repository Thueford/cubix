using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Hunter : EnemyBase
{
    public float startHP;

    // Start is called before the first frame update
    override protected void Start()
    {
        HP = startHP;
        base.Start();
    }

    // Update is called once per frame
    override protected void Update()
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
            if(dir.magnitude > 0) transform.forward = dir;
        }
    }
}
