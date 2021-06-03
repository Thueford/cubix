using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public static float timeCounter;
    private float 
        rateOfFire = .3f, 
        bulletSpeed = 30f, 
        spread = 30f;
    private const int amount = 5;
    public GameObject bulletPrefab;
    float radius;
    public static Vector3Int rgb = new Vector3Int(0, 0, 0);
    private Color
        black = new Color(.3f, .3f, .3f, 1f),
        blue = new Color(0f, .3f, 1f, 1f),
        green = new Color(0f, .7f, 0f, 1f);
    private Color color = new Color(.3f, .3f, .3f, 1f);
    public bool active;

    // Start is called before the first frame update
    void Start()
    {
        timeCounter = rateOfFire;
        radius = GetComponent<SphereCollider>().radius;
    }

    // Update is called once per frame
    void Update()
    {
        timeCounter += Time.deltaTime;
        if (timeCounter > rateOfFire)
        {
            if (active && Input.GetMouseButton(0))
            {
                timeCounter = 0;
                shoot(transform.forward);
            } else
                timeCounter = rateOfFire;
        }

        updateProperties();
    }

    void updateProperties()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            rgb.x = (~rgb.x) & 1;
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            rgb.y = (~rgb.y) & 1;
            bulletSpeed *= (rgb.y == 1 ? 1.8f : (1 / 1.8f));
            rateOfFire *= (rgb.y == 1 ? 2f : (1 / 2f));
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        { 
            rgb.z = (~rgb.z) & 1;
            rateOfFire *= (rgb.z == 1 ? 2/3f : 3/2f);
        }

        if (rgb == Vector3Int.zero) color = black;
        else if (rgb == Vector3Int.forward) color = blue;
        else if (rgb == Vector3Int.up) color = green;
        else color = new Color(rgb.x, rgb.y, rgb.z, 1f);
    }

    void shoot(Vector3 dir)
    {
        int reflects = rgb.y == 1 ? 2 : 0;
        int hits = rgb.y == 1 ? 4 : 0;
        float damage = rgb.y == 1 ? 2f : 1f;
        damage *= rgb.z == 1 ? 0.5f : 1f;

        if (rgb.z == 0)
        {
            CreateAndLaunch(dir, reflects, hits);
        } else
        {
            float ang = spread / (amount-1);
            for (float a = -spread/2; a <= spread/2; a += ang)
                CreateAndLaunch(Quaternion.Euler(0, a, 0) * dir, reflects, hits);
        }
    }

    void CreateAndLaunch(Vector3 dir, int reflects, int hits)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position + dir * radius, Quaternion.identity);
        bullet.GetComponent<Bullet>().launch(dir, bulletSpeed);
        bullet.GetComponent<Bullet>().setProperties(reflects, hits, color);
    }
}
