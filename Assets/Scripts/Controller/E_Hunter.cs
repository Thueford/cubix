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

    private const float MAXDIST_E = 20;

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
        if (movable)
        {
            Vector3 dir = 2 * (Player.self.transform.position - transform.position).normalized;
            
            // avoid other enemies
            foreach(EnemyBase e in Player.curStage.GetComponentsInChildren<EnemyBase>())
            {
                if(!e.Equals(this))
                {
                    Vector3 d = e.transform.position - transform.position;
                    if (d.magnitude < MAXDIST_E) dir -= (1 - d.magnitude / MAXDIST_E) * d.normalized;
                }
            }

            dir = dir.normalized * accelerationForce;

            // apply direction
            rb.AddForce(dir * Time.deltaTime * 1000, ForceMode.Acceleration);
            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;

            // look in movement direction
            dir.y = 0;
            if(dir.magnitude > 0) transform.forward = dir;
        }
    }
}
