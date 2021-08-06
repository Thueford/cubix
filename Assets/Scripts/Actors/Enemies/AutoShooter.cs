using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AutoShooter : ShooterBase
{
    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        timeCounter = Random.Range(0f, rateOfFire);
        if (bulletProps.color.a == 0) bulletProps.color = GameState.V2Color(Vector3Int.zero);
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
        tryShot();
    }
}
