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
        p.explosionRadius = 6f;
        p.color = GameState.V2Color(Vector3Int.zero);
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
    }

    public void updateProperties(Vector3Int rgbNew)
    {
        if (rgbNew.x != rgb.x) toggleRed(rgbNew.x == 1);
        if (rgbNew.y != rgb.y) toggleGreen(rgbNew.y == 1);
        if (rgbNew.z != rgb.z) toggleBlue(rgbNew.z == 1);
        p.color = GameState.V2Color(rgbNew);
        rgb = rgbNew;
    }

    private float cndinv(float f, bool b) => b ? f : 1/f;

    public void toggleRed(bool b) {
        p.explodes = b;
        p.speed *= cndinv(2/3f, b);
    }

    public void toggleGreen(bool b)
    {
        p.speed *= cndinv(3, b);
        rateOfFire *= cndinv(1.5f, b);
        p.damage *= cndinv(2, b);
        p.reflects = b ? 2 : 0;
        p.hits = b ? 4 : 0;
    }

    public void toggleBlue(bool b)
    {
        rateOfFire *= cndinv(2/3f, b);
        p.damage *= cndinv(1/3f, b);
        singleFire = !b;
    }

    public void atkSpeedBoost(float duration, float mult)
    {
        StartCoroutine(E_AtkSpeedBoost(duration, mult));
    }

    private IEnumerator E_AtkSpeedBoost(float duration, float mult)
    {
        rateOfFire /= mult;
        while (duration > 0)
        {
            if (!GameState.paused) duration -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        rateOfFire *= mult;
    }

    override protected void shoot(Vector3 dir)
    {
        base.shoot(dir);
        GameState.save.stats.firedBullets += singleFire ? 1 : amount;
        if (rgb.x == 1) GameState.save.stats.firedRedShots++;
        if (rgb.y == 1) GameState.save.stats.firedGreenShots++;
        if (rgb.z == 1) GameState.save.stats.firedBlueShots++;
        //Player.self.KnockBack(-dir.normalized * p.speed * 3);
    }
}
