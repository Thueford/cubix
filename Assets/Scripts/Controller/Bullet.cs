using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 dir, oldVelocity;
    private Vector3Int rgb;
    private float speed;
    private int reflects, hits;
    private Rigidbody rb;
    private float damage, radius;
    public CapsuleCollider cc;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        radius = gameObject.GetComponent<SphereCollider>().radius;
        if (dir != null)
            rb.velocity = dir * speed;
        //cc = GetComponentInChildren<CapsuleCollider>();
        if (cc == null) Debug.Log("No CapsuleCollider on Bullet");
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
        float distanceToTravel = speed * Time.fixedDeltaTime;

        cc.height = distanceToTravel * 2;
        cc.transform.rotation = Quaternion.LookRotation(rb.velocity);
        cc.center = new Vector3(0f, 0f, distanceToTravel);

        if (Physics.SphereCast(transform.position, radius, rb.velocity, out RaycastHit hitInfo, distanceToTravel, layerMask: (1 << 14) | (1 << 8)))
        {
            //Debug.Log("Hit SphereCast");
            if (hitInfo.collider.CompareTag("Enemy"))
                hitTarget(hitInfo.collider);
        }
    }

    public void setProperties(int reflects, int hits, float damage, Color color, string owner)
    {
        cc.gameObject.layer = owner == "Player" ? 16 : 15;
        this.reflects = reflects;
        this.hits = hits;
        this.damage = damage;
        setColor(color);
    }

    private void setColor(Color color)
    {
        gameObject.GetComponent<Renderer>().material.color = color;
        gameObject.GetComponent<Light>().color = color == GameState.black ? GameState.glow : color;
    }

    public void launch(Vector3 dir, float speed)
    {
        this.dir = dir;
        this.speed = speed;
        if (rb) rb.velocity = this.dir * this.speed;
    }

    private void explode()
    {
        //Animate

    }

    //Source: https://answers.unity.com/questions/352609/how-can-i-reflect-a-projectile.html
    void OnCollisionEnter(Collision c)
    {
        if (rgb.x == 1) explode();

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
        rb.velocity = oldVelocity = reflectedVelocity;

        /*
        rotate the object by the same ammount we changed its velocity
        Quaternion rotation = Quaternion.FromToRotation(oldVelocity, reflectedVelocity);
        transform.rotation = rotation * transform.rotation;
        */
    }

    private void hitTarget(Collider c)
    {
        //Debug.Log("BTrigger: " + c.name + " " + tag + " " + c.tag);
        if (!CompareTag(c.tag))
        {
            EntityBase b = c.GetComponentInParent<EntityBase>();
            if (b)
            {
                Debug.Log("bullet hit Enemy");
                b.Hit(damage);
                if (--hits < 0) Destroy(gameObject);
                // if (--reflects < 0) Destroy(gameObject);
            }
            else Debug.LogWarning("bullet hit non-Entity");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bullet Bullet Collision: " + other);
    }
}
