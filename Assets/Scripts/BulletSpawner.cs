using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public static float timeCounter;
    private const float rateOfFire = .5f;
    private const float bulletSpeed = 5f;
    private const float spread = 30f;
    private const int amount = 5;
    public GameObject bulletPrefab;
    float radius;
    public static Vector3Int rgb = new Vector3Int(0, 1, 0);

    // Start is called before the first frame update
    void Start()
    {
        timeCounter = 0;
        radius = gameObject.GetComponent<SphereCollider>().radius;
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
    }

    void spawnBullet(Vector3 dir)
    {
        if (rgb.z == 0)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position + dir * radius, Quaternion.identity);
            bullet.GetComponent<BulletController>().launch(dir, bulletSpeed);
        } else
        {
            float ang = spread / (amount-1);
            for (float f = -spread/2; f <= spread/2; f+= ang)
            {
                Vector3 newDir = Quaternion.Euler(0, f, 0) * dir;
                GameObject bullet = Instantiate(bulletPrefab, transform.position + newDir * radius, Quaternion.identity);
                bullet.GetComponent<BulletController>().launch(newDir, bulletSpeed);
            }
        }
    }
}
