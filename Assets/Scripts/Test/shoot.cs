using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shoot : MonoBehaviour
{
    public bool useForce;
    public float speed;
    public float force;
    public Vector3 dir;
    public Rigidbody rb;
    public CapsuleCollider cc;

    private bool used = false;
    private Vector3 oldVelocity;
    private float radius;
    // Start is called before the first frame update
    void Start()
    {
        radius = gameObject.GetComponent<SphereCollider>().radius;
        if (cc == null) Debug.Log("No CapsuleCollider on Bullet");
    }

    // Update is called once per frame
    void Update()
    {
        oldVelocity = rb.velocity;
        if (Input.GetKey(KeyCode.Space) )
        {
            Time.timeScale = 1 / 60f;
            if (!useForce)
            {
                rb.velocity = dir.normalized * speed;
            }
            else
            {
                rb.AddForce(dir.normalized * force * rb.mass);
            }
            used = true;
        }
    }

    private void FixedUpdate()
    {
        float distanceToTravel = rb.velocity.magnitude * Time.fixedDeltaTime;

        cc.height = distanceToTravel;
        cc.transform.rotation = Quaternion.LookRotation(rb.velocity);
        cc.center = new Vector3(0f, 0f, distanceToTravel/2);

        if (Physics.SphereCast(transform.position, radius, rb.velocity, out RaycastHit hitInfo, distanceToTravel, layerMask: (1 << 14) | (1 << 8)))
        {
            //Debug.Log("Hit SphereCast");
            if (hitInfo.collider.CompareTag("Enemy"))
                Debug.Log("Enemy");
                //hitTarget(hitInfo.collider);
        }
    }

    void OnCollisionEnter(Collision c)
    {
        Debug.Log("Collision: " + c.collider.name + " " + c.ToString());
        // get the point of contact
        ContactPoint contact = c.contacts[0];

        // reflect our old velocity off the contact point's normal vector
        Vector3 reflectedVelocity = Vector3.Reflect(oldVelocity, contact.normal);

        // assign the reflected velocity back to the rigidbody
        rb.velocity = oldVelocity = reflectedVelocity;
    }

    private void OnTriggerEnter(Collider c)
    {
        Debug.Log("BTrigger: " + c.name + " " + tag + " " + c.tag);
        /*if (!CompareTag(c.tag))
        {
            EntityBase b = c.GetComponentInParent<EntityBase>();
            if (b)
            {
                Debug.Log("bullet hit Enemy");
            }
            else Debug.LogWarning("bullet hit non-Entity");
        }*/
    }
}