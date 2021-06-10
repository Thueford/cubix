using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : ShooterBase
{
    

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
        p.explosionRadius = 0f;
        p.color = new Color(.3f, .3f, .3f, 1f);
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
    }

    public void updateColor(Vector3Int rgb)
    {
        if (rgb == Vector3Int.zero) p.color = GameState.black;
        else if (rgb == Vector3Int.forward) p.color = GameState.blue;
        else if (rgb == Vector3Int.up) p.color = GameState.green;
        else p.color = new Color(rgb.x, rgb.y, rgb.z, 1f);
    }

    public void updateProperties(Vector3Int rgbNew, Vector3Int rgbOld)
    {
        if (rgbNew.x != rgbOld.x) toggleRed(rgbNew.x == 1);
        if (rgbNew.y != rgbOld.y) toggleGreen(rgbNew.y == 1);
        if (rgbNew.z != rgbOld.z) toggleBlue(rgbNew.z == 1);
    }

    public void toggleRed(bool b) {
        p.explodes = b;
        p.speed *= (b ? 3/2f : 2/3f);
    }

    public void toggleGreen(bool b)
    {
        p.speed *= (b ? 3f : 1/3f);
        rateOfFire *= (b ? 2f : 1/2f);
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

}
