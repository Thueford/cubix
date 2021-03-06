using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SphereCollider), typeof(Renderer), typeof(Light))]
public class Bullet : MonoBehaviour
{
    [NotNull] public Rigidbody rb;
    [NotNull] public CapsuleCollider cc;
    [NotNull] public Explosion explosionPrefab;
    [NotNull] public GameObject hitPrefab;

    private Vector3 dir, oldVelocity;
    private float radius, velocityMultiplier;
    private Properties p;

    [System.Serializable]
    public struct Properties
    {
        public bool explodes;
        public int reflects, hits;
        public float speed, damage, explosionRadius;
        public string owner;
        public Color color;
    }

    void Awake()
    {
        radius = GetComponent<SphereCollider>().radius;
    }

    // Start is called before the first frame update
    void Start()
    {
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
        else if (rb.velocity.magnitude < p.speed)
        {
            rb.velocity = rb.velocity.normalized * p.speed;
        }
        oldVelocity = rb.velocity;
    }

    Vector3 storeVel;
    public void Freeze()
    {
        storeVel = rb.velocity;
        rb.velocity = Vector3.zero;
    }

    public void Melt()
    {
        rb.velocity = storeVel;
    }

    private void FixedUpdate()
    {
        float distanceToTravel = rb.velocity.magnitude * velocityMultiplier;

        cc.height = distanceToTravel + radius*2;
        cc.center = new Vector3(0f, 0f, distanceToTravel/2);
    }

    public void setProperties(Properties p)
    {
        this.p = p;
        setColor(p.color);
        cc.gameObject.layer = p.owner == "Player" ? 16 : 15;
        tag = cc.tag = p.owner + "Bullet";
    }

    private void setColor(Color color)
    {
        GetComponent<Renderer>().material.color = color;
        GetComponent<Light>().color = GameState.getLightColor(color);
        GetComponent<Particles>().color.color = GameState.getLightColor(color);
    }

    public void launch(Vector3 dir)
    {
        this.dir = dir;
        if (rb) rb.velocity = this.dir * p.speed;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void hit()
    {
        Instantiate(hitPrefab, transform.position, Quaternion.identity, GameState.self.effectContainer);
    }

    private void explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity, GameState.self.effectContainer)
            .SetProperties(p);
    }


    //Source: https://answers.unity.com/questions/352609/how-can-i-reflect-a-projectile.html
    void OnCollisionEnter(Collision c)
    {
        if (p.explodes) explode();
        else { hit(); SoundHandler.PlayClip("wallHit"); }

        if (--p.reflects < 0)
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
        EntityBase b = c.GetComponentInParent<EntityBase>();

        if (c.CompareTag("Enemy") && CompareTag("PlayerBullet"))
        {
            if (!b) Debug.LogWarning("bullet hit non-Entity");
            else if (!p.explodes)
            {
                SoundHandler.PlayClip(SoundHandler.clips["enemyHit"]);
                b.Hit(p.damage);
                if (!((EnemyBase)b).isBoss)
                    b.KnockBack(rb.velocity.normalized*(5+p.speed*0.15f));
            }
        }
        else if (c.CompareTag("Player") && CompareTag("EnemyBullet"))
        {
            if (!b) Debug.LogWarning("bullet hit non-Entity");
            else if (!p.explodes) b.Hit(p.damage);
        }
        else if (c.CompareTag("EnemyBullet") && CompareTag("PlayerBullet"))
        {
            SoundHandler.PlayClip("wallHit");
        }

        if (p.explodes) explode();
        else hit(); 
        if (--p.hits < 0) Destroy(gameObject);
    }
}
