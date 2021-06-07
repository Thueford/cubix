using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    private Color color = new Color(.3f, .3f, .3f, 1f);
    public bool active;
    private bool 
        singleFire = true,
        explodes = false;
    private int
        reflects = 0,
        hits = 0,
        amount = 5;
    private float
        rateOfFire = .3f,
        bulletSpeed = 20f,
        damage = 1f,
        spread = 30f,
        explosionRadius = 0f,
        timeCounter,
        shooterRadius;

    // Start is called before the first frame update
    void Start()
    {
        timeCounter = rateOfFire;
        shooterRadius = GetComponent<SphereCollider>().radius;
    }

    // Update is called once per frame
    void Update()
    {
        timeCounter = Mathf.Min(timeCounter + Time.deltaTime, rateOfFire);
    }

    public void updateColor(Vector3Int rgb)
    {
        if (rgb == Vector3Int.zero) color = GameState.black;
        else if (rgb == Vector3Int.forward) color = GameState.blue;
        else if (rgb == Vector3Int.up) color = GameState.green;
        else color = new Color(rgb.x, rgb.y, rgb.z, 1f);
    }

    public void toggleRed(bool b) {

    }

    public void toggleGreen(bool b)
    {
        bulletSpeed *= (b ? 3f : (1 / 3f));
        rateOfFire *= (b ? 2f : (1 / 2f));
        reflects = b ? 2 : 0;
        hits = b ? 4 : 0;
        damage *= b ? 2f : 0.5f;
    }

    public void toggleBlue(bool b)
    {
        rateOfFire *= b ? 2 / 3f : 3 / 2f;
        damage *= b ? 0.5f : 2f;
        singleFire = !b;
    }

    public void tryShot()
    {
        if (active && timeCounter >= rateOfFire)
        {
            timeCounter = 0;
            shoot(transform.forward);
        }
    }

    void shoot(Vector3 dir)
    {
        if (singleFire)
        {
            CreateAndLaunch(dir);
        } else
        {
            float ang = spread / (amount-1);
            for (float a = -spread/2; a <= spread/2; a += ang)
                CreateAndLaunch(Quaternion.Euler(0, a, 0) * dir);
        }
    }

    void CreateAndLaunch(Vector3 dir)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position + dir * shooterRadius, Quaternion.identity);
        bullet.GetComponent<Bullet>().launch(dir, bulletSpeed);
        bullet.GetComponent<Bullet>().setProperties(reflects, hits, damage, color, gameObject.tag);
        bullet.tag = tag + "Bullet";
    }
}
