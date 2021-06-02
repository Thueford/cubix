using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    private Vector3 dir;
    private Vector3 oldVelocity;
    private float speed;
    private Rigidbody rb;
    private Vector3Int rgb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (dir != null) 
            rb.velocity = this.dir * this.speed;
        rgb = BulletSpawner.rgb;
    }

    // Update is called once per frame
    void Update()
    {
        oldVelocity = rb.velocity;
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
        if (rgb.y == 0)
        {
            if (rgb.x == 1)
                explode();
            Destroy(this.gameObject);
            return;
        }
        // get the point of contact
        ContactPoint contact = collision.contacts[0];

        // reflect our old velocity off the contact point's normal vector
        Vector3 reflectedVelocity = Vector3.Reflect(oldVelocity, contact.normal);

        // assign the reflected velocity back to the rigidbody
        rb.velocity = reflectedVelocity;

        /*
         * rotate the object by the same ammount we changed its velocity
        Quaternion rotation = Quaternion.FromToRotation(oldVelocity, reflectedVelocity);
        transform.rotation = rotation * transform.rotation;
        */
    }
}
