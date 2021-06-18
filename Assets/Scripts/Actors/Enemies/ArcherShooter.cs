using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ArcherShooter : ShooterBase
{
    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();

        singleFire = true;
        amount = 1;
        rateOfFire = 1.5f;
        spread = 30f;
        timeCounter = Random.Range(0f, rateOfFire);

        p.explodes = false;
        p.reflects = 0;
        p.hits = 0;
        p.speed = 10f;
        p.damage = 1f;
        p.explosionRadius = 0f;
        p.color = new Color(.3f, .3f, .3f, 1f);
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
        tryShot();
    }

}
