using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShooterBase : MonoBehaviour
{
    public bool active;
    [NotNull] public Bullet bulletPrefab;

    public int amount;
    public float rateOfFire, spread;
    public Bullet.Properties bulletProps;
    protected float timeCounter, shooterRadius;

    virtual protected void Awake()
    {
        shooterRadius = GetComponent<SphereCollider>().radius;
    }

    // Start is called before the first frame update
    virtual protected void Start()
    {
        bulletProps.owner = gameObject.tag;
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
        if (amount <= 1) CreateAndLaunch(dir);
        else
        {
            float ang = spread / (amount - 1);
            for (float a = -spread / 2; a <= spread / 2; a += ang)
                CreateAndLaunch(Quaternion.Euler(0, a, 0) * dir);
        }
    }

    virtual protected void CreateAndLaunch(Vector3 dir)
    {
        Vector3 pos = transform.position + dir * shooterRadius;
        pos.y = .5f;
        Bullet bullet = Instantiate(bulletPrefab, pos, Quaternion.identity, GameState.curStage.actors.transform);
        bullet.setProperties(bulletProps);
        bullet.launch(dir);
        //bullet.tag = tag + "Bullet";
    }
}
