using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public static float timeCounter;
    private const float 
        rateOfFire = .2f, 
        bulletSpeed = 25f, 
        spread = 30f;
    private const int amount = 5;
    public GameObject bulletPrefab;
    float radius;
    public static Vector3Int rgb = new Vector3Int(0, 0, 0);

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
            if (Input.GetMouseButton(0))
            {
                timeCounter = 0;
                spawnBullet(transform.forward);
            } else
                timeCounter = rateOfFire;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
            rgb.x = (~rgb.x) & 1;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            rgb.y = (~rgb.y) & 1;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            rgb.z = (~rgb.z) & 1;
    }

    void spawnBullet(Vector3 dir)
    {
        float correctedSpeed = bulletSpeed * (rgb.y == 1 ? 1.2f : 1f);
        if (rgb.z == 0)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position + dir * radius, Quaternion.identity);
            bullet.GetComponent<Bullet>().launch(dir, correctedSpeed);
        } else
        {
            float ang = spread / (amount-1);
            for (float f = -spread/2; f <= spread/2; f += ang)
            {
                Vector3 newDir = Quaternion.Euler(0, f, 0) * dir;
                GameObject bullet = Instantiate(bulletPrefab, transform.position + newDir * radius, Quaternion.identity);
                bullet.GetComponent<Bullet>().launch(newDir, correctedSpeed);
            }
        }
    }
}
