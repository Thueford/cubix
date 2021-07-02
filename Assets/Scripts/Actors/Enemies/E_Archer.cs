using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Archer : EnemyBase
{
    public static float
        AP_FAC_NOISE = 1.5f;

    public static Effector_T aplayer =
        new Effector_T("Player", -3.5f, 40, true);

    override public Vector3 steer()
    {
        List<Vector3> effectors = new List<Vector3>(10);

        // keep distance from player
        Effector_T aplayerc = aplayer;
        aplayerc.factor += AP_FAC_NOISE * perlinNoise(noiseTime, pnD);
        effectors.Add(aplayerc.getEff(this, Player.self));

        // avoid enemies
        return contextSteer2Player(effectors);
    }

    override public void Update()
    {
        base.Update();

        // look at player
        //if(effectors[0].magnitude > 0) transform.forward = effectors[0];
        transform.forward = Player.self.transform.position - transform.position;
    }
}
