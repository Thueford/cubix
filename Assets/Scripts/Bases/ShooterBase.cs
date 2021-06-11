using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShooterBase : MonoBehaviour
{
    public bool active;
    [NotNull] public GameObject bulletPrefab;

    protected bool singleFire;
    protected int amount;
    protected float timeCounter, rateOfFire, spread, shooterRadius;
    protected Bullet.Properties p;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        p.owner = gameObject.tag;
        shooterRadius = GetComponent<SphereCollider>().radius;
        timeCounter = rateOfFire;
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        timeCounter = Mathf.Min(timeCounter + Time.deltaTime, rateOfFire);
    }

    virtual public void tryShot()
    {
        if (active && timeCounter >= rateOfFire)
        {
            timeCounter = 0;
            shoot(transform.forward);
        }
    }

    virtual protected void shoot(Vector3 dir)
    {
        if (singleFire)
        {
            CreateAndLaunch(dir);
        }
        else
        {
            float ang = spread / (amount - 1);
            for (float a = -spread / 2; a <= spread / 2; a += ang)
                CreateAndLaunch(Quaternion.Euler(0, a, 0) * dir);
        }
    }

    virtual protected void CreateAndLaunch(Vector3 dir)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position + dir * shooterRadius, Quaternion.identity);
        bullet.GetComponent<Bullet>().setProperties(p);
        bullet.GetComponent<Bullet>().launch(dir);
        //bullet.tag = tag + "Bullet";
    }
}
