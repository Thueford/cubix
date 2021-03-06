using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerShooter : ShooterBase
{

    private Vector3Int rgb = Vector3Int.zero;
    public Vector3Int lastColor { get; private set; }

    private Dictionary<float, int> atkSpdMults = new Dictionary<float, int>();

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
    }

    public void updateProperties(Vector3Int rgbNew)
    {
        if (rgbNew.sqrMagnitude == 1) lastColor = rgbNew;
        if (rgbNew.x != rgb.x) toggleRed(rgbNew.x == 1);
        if (rgbNew.y != rgb.y) toggleGreen(rgbNew.y == 1);
        if (rgbNew.z != rgb.z) toggleBlue(rgbNew.z == 1);
        bulletProps.color = GameState.V2Color(rgbNew);
        rgb = rgbNew;

        amount = rgb.z == 1 ? ((rgb - Vector3Int.forward).sqrMagnitude == 0 ? 5 : 3) : 1;
    }

    private float cndinv(float f, bool b) => b ? f : 1/f;

    public void toggleRed(bool b)
    {
        bulletProps.explodes = b;
        bulletProps.speed *= cndinv(2/3f, b);
    }

    public void toggleGreen(bool b)
    {
        bulletProps.speed *= cndinv(3, b);
        rateOfFire *= cndinv(1.5f, b);
        bulletProps.damage *= cndinv(2, b);
        bulletProps.reflects = b ? 2 : 0;
        bulletProps.hits = b ? 4 : 0;
    }

    public void toggleBlue(bool b)
    {
        rateOfFire *= cndinv(2/3f, b);
        bulletProps.damage *= cndinv(1/3f, b);
    }

    public void atkSpeedBoost(float duration, float mult)
    {
        StartCoroutine(E_AtkSpeedBoost(duration, mult));
    }

    public void atkSpeedBoost(float mult) 
    { 
        rateOfFire /= mult;
        if (atkSpdMults.ContainsKey(mult))
            atkSpdMults[mult] += 1;
        else
            atkSpdMults.Add(mult, 1);
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
        PlayerStats.self.firedBullets += amount;
        if (rgb.x == 1) PlayerStats.self.firedRedShots++;
        if (rgb.y == 1) PlayerStats.self.firedGreenShots++;
        if (rgb.z == 1) PlayerStats.self.firedBlueShots++;
        StageStats.cur.AddShot((Vector4)(Vector3)rgb);
        //Player.self.KnockBack(-dir.normalized * p.speed * 3);
    }

    public Dictionary<float, int> getAtkSpd()
    {
        return new Dictionary<float, int>(atkSpdMults);
    }

    public void setAtkSpd(Dictionary<float, int> mults)
    {
        if (mults != atkSpdMults)
        {
            foreach (float m in atkSpdMults.Keys)
                rateOfFire *= Mathf.Pow(m, atkSpdMults[m]);
            foreach (float m in mults.Keys)
                rateOfFire /= Mathf.Pow(m, mults[m]);
        }
    }
}
