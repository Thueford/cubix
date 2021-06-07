using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private bool explodes;
    private Vector3 dir, oldVelocity;
    private float speed;
    private int reflects, hits;
    public Rigidbody rb;
    private float damage, radius, velocityMultiplier;
    public CapsuleCollider cc;

    // Start is called before the first frame update
    void Start()
    {
        if (!rb) Debug.LogWarning("No Rigidbody assigned to Bullet Script");
        if (cc == null) Debug.LogWarning("No CapsuleCollider on Bullet");
        radius = gameObject.GetComponent<SphereCollider>().radius;
        velocityMultiplier = Time.fixedDeltaTime / cc.transform.lossyScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.magnitude < .5)
        {
            Debug.Log("Bullet Stuck");
            Destroy(gameObject);
        }
        else if (rb.velocity.magnitude < speed)
        {
            rb.velocity = rb.velocity.normalized * speed;
        }
        oldVelocity = rb.velocity;
    }

    private void FixedUpdate()
    {
        float distanceToTravel = rb.velocity.magnitude * velocityMultiplier;

        cc.height = distanceToTravel + radius*2;
        cc.center = new Vector3(0f, 0f, distanceToTravel/2);

        /*
        if (Physics.SphereCast(transform.position, radius, rb.velocity, out RaycastHit hitInfo, distanceToTravel, layerMask: (1 << 14) | (1 << 8)))
        {
            //Debug.Log("Hit SphereCast");
            if (hitInfo.collider.CompareTag("Enemy"))
                hitTarget(hitInfo.collider);
        }
        */
    }

    public void setProperties(BulletSpawner.Properties p)
    {
        cc.gameObject.layer = p.owner == "Player" ? 16 : 15;
        tag = p.owner + "Bullet";
        reflects = p.reflects;
        hits = p.hits;
        damage = p.damage;
        speed = p.bulletSpeed;
        explodes = p.explodes;
        setColor(p.color);
    }

    private void setColor(Color color)
    {
        gameObject.GetComponent<Renderer>().material.color = color;
        gameObject.GetComponent<Light>().color = color == GameState.black ? GameState.glow : color;
    }

    public void launch(Vector3 dir)
    {
        this.dir = dir;
        if (rb) rb.velocity = this.dir * speed;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void explode()
    {
        //Animate

    }

    //Source: https://answers.unity.com/questions/352609/how-can-i-reflect-a-projectile.html
    void OnCollisionEnter(Collision c)
    {
        if (explodes) explode();

        if (--reflects < 0)
        {
            Destroy(gameObject);
            return;
        }

        // get the point of contact
        ContactPoint contact = c.contacts[0];

        // reflect our old velocity off the contact point's normal vector
        Vector3 reflectedVelocity = Vector3.Reflect(oldVelocity, contact.normal);

        // assign the reflected velocity back to the rigidbody
        rb.velocity = reflectedVelocity;

        //rotate the object by the same ammount we changed its velocity
        Quaternion rotation = Quaternion.FromToRotation(oldVelocity, reflectedVelocity);
        transform.rotation = rotation * transform.rotation;

        oldVelocity = reflectedVelocity;
    }

    private void OnTriggerEnter(Collider c)
    {
        //Debug.Log("BTrigger: " + c.name + " " + tag + " " + c.tag);
        if (c.CompareTag("Enemy") && CompareTag("PlayerBullet"))
        {
            EntityBase b = c.GetComponentInParent<EntityBase>();
            if (b)
            {
                Debug.Log("bullet hit Enemy");
                b.Hit(damage);
                if (--hits < 0) Destroy(gameObject);
            }
            else Debug.LogWarning("bullet hit non-Entity");
        }
        else if (c.CompareTag("Player") && CompareTag("EnemyBullet"))
        {
            //Damage Player
            if (--hits < 0) Destroy(gameObject);
        }
        else if (c.CompareTag("EnemyBullet") && CompareTag("PlayerBullet"))
        {
            Debug.Log("BulletBulletCollision");
            Destroy(c.gameObject);
            if (--hits < 0) Destroy(gameObject);
        }
    }

}
