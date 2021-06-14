using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Hunter : EnemyBase
{

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        HP = startHP;
    }


    // Update is called once per frame
    override public void steer()
    {
        List<Vector3> effectors = new List<Vector3>(10);

        // avoid enemies
        effectors.AddRange(eenemy.getEffs(this));
        steerDir = contextSteer2Player(effectors) * accelerationForce;
    }

    override public void Update()
    {
        base.Update();

        // look in movement direction
        steerDir.y = 0;
        if (steerDir.magnitude > 0) transform.forward = steerDir;
    }
}
