using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Stray : EnemyBase
{
    override public Vector3 steer()
    {
        return contextSteerIDLE(new List<Vector3>(10));
    }

    override public void Update()
    {
        base.Update();

        // look in movement direction
        if (steerDir.sqrMagnitude > 0) transform.forward = steerDir;
    }
}
