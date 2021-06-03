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
        } else if (rb.velocity.magnitude < speed)
        {
            rb.velocity = rb.velocity.normalized * speed;
        }
        oldVelocity = rb.velocity;
    }

    public void setProperties(int reflects, int hits, Color color)
    {
        this.reflects = reflects;
        this.hits = hits;
        setColor(color);
    }

    private void setColor(Color color)
    {
        this.color = color;
        gameObject.GetComponent<Renderer>().material.color = color;
        gameObject.GetComponent<Light>().color = color;
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
     void OnCollisionEnter(Collision collision)
    {
        if (rgb.x == 1)
            explode();

        if (--reflects < 0)
        {
            Destroy(gameObject);
            return;
        }

        // get the point of contact
        ContactPoint contact = collision.contacts[0];

        // reflect our old velocity off the contact point's normal vector
        Vector3 reflectedVelocity = Vector3.Reflect(oldVelocity, contact.normal);

        // assign the reflected velocity back to the rigidbody
        rb.velocity = oldVelocity = reflectedVelocity;

        /*
         * rotate the object by the same ammount we changed its velocity
        Quaternion rotation = Quaternion.FromToRotation(oldVelocity, reflectedVelocity);
        transform.rotation = rotation * transform.rotation;
        */

    }
}
