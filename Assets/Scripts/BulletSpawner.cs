using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public static float timeCounter;
    private const float rateOfFire = .5f;
    private const float bulletSpeed = 5f;
    public GameObject bulletPrefab;
    float radius;
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
        GameObject bullet = Instantiate(bulletPrefab, transform.position + dir * radius, Quaternion.identity);
        bullet.GetComponent<BulletController>().launch(dir, bulletSpeed);

    }
}
