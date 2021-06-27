using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerShooter : ShooterBase
{

    private Vector3Int rgb = Vector3Int.zero;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();

        singleFire = true;
        amount = 5;
        rateOfFire = .3f;
        spread = 30f;

        p.explodes = false;
        p.reflects = 0;
        p.hits = 0;
        p.speed = 40f;
        p.damage = 1f;
        p.explosionRadius = 7.5f;
        p.color = GameState.V2Color(Vector3Int.zero);
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
    }

    private void updateColor(Vector3Int rgb)
    {
    }

    public void updateProperties(Vector3Int rgbNew)
    {
        if (rgbNew.x != rgb.x) toggleRed(rgbNew.x == 1);
        if (rgbNew.y != rgb.y) toggleGreen(rgbNew.y == 1);
        if (rgbNew.z != rgb.z) toggleBlue(rgbNew.z == 1);
        p.color = GameState.V2Color(rgbNew);
        rgb = rgbNew;
    }

    public void toggleRed(bool b) {
        p.explodes = b;
        //rateOfFire *= b ? 2 / 3f : 3 / 2f;
        p.speed *= (b ? 2/3f : 1.5f);
    }

    public void toggleGreen(bool b)
    {
        p.speed *= (b ? 3f : 1/3f);
        rateOfFire *= (b ? 1.5f : 2/3f);
        p.reflects = b ? 2 : 0;
        p.hits = b ? 4 : 0;
        p.damage *= b ? 2f : 1/2f;
    }

    public void toggleBlue(bool b)
    {
        rateOfFire *= b ? 2 / 3f : 3 / 2f;
        p.damage *= b ? 1/2f : 2f;
        singleFire = !b;
    }

    override protected void shoot(Vector3 dir)
    {
        base.shoot(dir);
        //Player.self.KnockBack(-dir.normalized * p.speed * 3);
    }
}
