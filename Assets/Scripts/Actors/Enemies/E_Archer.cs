using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Archer : EnemyBase
{
    public static float
        MINDIST_P_NOISE = 10;

    public static Effector_T aplayer =
        new Effector_T("Player", -5, 20, false);

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        HP = startHP;
    }

    // Update is called once per frame
    override public void steer()
    {
        if (movable)
        {
            // EnemyBase[] enemies = Player.curStage.GetComponentsInChildren<EnemyBase>();
            List<Vector3> effectors = new List<Vector3>(10);

            // keep distance from player
            Effector_T aplayerc = aplayer;
            aplayerc.dist += MINDIST_P_NOISE * perlinNoise(noiseTime, pnD);
            effectors.Add(aplayerc.getEff(this, Player.self));
            dbgLine(aplayerc.getEff(this, Player.self) + eplayer.getEff(this, Player.self), aplayerc.dist + eplayer.dist, Color.white);

            // avoid enemies
            effectors.AddRange(eenemy.getEffs(this));
            steerDir = contextSteer2Player(effectors) * accelerationForce;
        }
    }

    override public void Update()
    {
        base.Update();

        // look at player
        //if(effectors[0].magnitude > 0) transform.forward = effectors[0];
        transform.forward = Player.self.transform.position - transform.position;
    }
}
