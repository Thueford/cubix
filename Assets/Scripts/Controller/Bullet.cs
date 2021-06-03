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
    private Color color;
    private float damage;
    private static Color
        black = new Color(.3f, .3f, .3f, 1f),
        glow = new Color(.7f, .7f, .7f, 1f);
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rgb = BulletSpawner.rgb;
        if (dir != null)
            rb.velocity = dir * speed;
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

    public void setProperties(int reflects, int hits, float damage, Color color)
    {
        this.reflects = reflects;
        this.hits = hits;
        this.damage = damage;
        setColor(color);
    }

    private void setColor(Color color)
    {
        this.color = color;
        gameObject.GetComponent<Renderer>().material.color = color;
        gameObject.GetComponent<Light>().color = color == black ? glow : color;
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
        if (!c.collider.CompareTag("Wall")) return;

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

    private void OnTriggerEnter(Collider c)
    {
        Debug.Log("BTrigger: " + c.name + " " + tag + " " + c.tag);
        if (!CompareTag(c.tag))
        {
            EntityBase b = c.GetComponent<EntityBase>();
            if (b)
            {
                b.Hit(damage);
                if (--hits < 0) Destroy(gameObject);
                // if (--reflects < 0) Destroy(gameObject);
            }
            else Debug.LogWarning("bullet hit non-Entity");
        }
    }
}
