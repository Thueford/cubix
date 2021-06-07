using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    //private Color color = new Color(.3f, .3f, .3f, 1f);
    public bool active;
    private float timeCounter;
    
    public struct Properties
    {
        public bool singleFire, explodes;
        public int reflects, hits, amount;
        public float rateOfFire, bulletSpeed, damage,
            spread, explosionRadius, shooterRadius;
        public string owner;
        public Color color;
    }

    public Properties p;

    // Start is called before the first frame update
    void Start()
    {
        p.singleFire = true;
        p.explodes = false;
        p.reflects = 0;
        p.hits = 0;
        p.amount = 5;
        p.rateOfFire = .3f;
        p.bulletSpeed = 40f;
        p.damage = 1f;
        p.spread = 30f;
        p.explosionRadius = 0f;
        p.shooterRadius = GetComponent<SphereCollider>().radius;
        p.owner = gameObject.tag;
        p.color = new Color(.3f, .3f, .3f, 1f);
        timeCounter = p.rateOfFire;
        
    }

    // Update is called once per frame
    void Update()
    {
        timeCounter = Mathf.Min(timeCounter + Time.deltaTime, p.rateOfFire);
    }

    public void updateColor(Vector3Int rgb)
    {
        if (rgb == Vector3Int.zero) p.color = GameState.black;
        else if (rgb == Vector3Int.forward) p.color = GameState.blue;
        else if (rgb == Vector3Int.up) p.color = GameState.green;
        else p.color = new Color(rgb.x, rgb.y, rgb.z, 1f);
    }

    public void toggleRed(bool b) {

    }

    public void toggleGreen(bool b)
    {
        p.bulletSpeed *= (b ? 3f : (1 / 3f));
        p.rateOfFire *= (b ? 2f : (1 / 2f));
        p.reflects = b ? 2 : 0;
        p.hits = b ? 4 : 0;
        p.damage *= b ? 2f : 0.5f;
    }

    public void toggleBlue(bool b)
    {
        p.rateOfFire *= b ? 2 / 3f : 3 / 2f;
        p.damage *= b ? 0.5f : 2f;
        p.singleFire = !b;
    }

    public void tryShot()
    {
        if (active && timeCounter >= p.rateOfFire)
        {
            timeCounter = 0;
            shoot(transform.forward);
        }
    }

    void shoot(Vector3 dir)
    {
        if (p.singleFire)
        {
            CreateAndLaunch(dir);
        } else
        {
            float ang = p.spread / (p.amount-1);
            for (float a = -p.spread/2; a <= p.spread /2; a += ang)
                CreateAndLaunch(Quaternion.Euler(0, a, 0) * dir);
        }
    }

    void CreateAndLaunch(Vector3 dir)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position + dir * p.shooterRadius, Quaternion.identity);
        bullet.GetComponent<Bullet>().setProperties(p);
        bullet.GetComponent<Bullet>().launch(dir);
        //bullet.tag = tag + "Bullet";
    }
}
