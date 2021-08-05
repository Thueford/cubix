using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaloShooter : ShooterBase
{
    private int shots = 0;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        bulletProps.color = GameState.V2Color(Vector3Int.zero);
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
        if (shots > 0) tryShot();
    }

    public void activate(float secs)
    {
        shots += (int)(secs /rateOfFire);
    }

    override protected void shoot(Vector3 dir)
    {
        base.shoot(dir);
        --shots;
    }
}
