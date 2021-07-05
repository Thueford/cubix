using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [NotNull] public SphereCollider sc;
    [NotNull] public Particles ps;
    [NotNull] public GameObject hitPrefab;
    private float lifespan = 0.1f, age = 0, damage = 0;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("StopCollision", lifespan);
    }

    // destroyed by ParticleSystem
    // void StopExplosion() { Destroy(gameObject); }
    void StopCollision() { sc.enabled = false; }

    public void SetProperties(Bullet.Properties p)
    {
        gameObject.layer = p.owner == "Player" ? 16 : 15;
        tag = p.owner + "Bullet";
        sc.radius = p.explosionRadius;

        ps.vel.scale = 3 * sc.radius * ps.vel.scale.normalized;
        ps.properties.lifetime = sc.radius / ps.vel.scale.x;
        ps.size.val.z = -ps.size.val.y / ps.properties.lifetime;

        // light.range = p.explosionRadius*2;
        damage = p.damage;
    }

    public void SetProperties(string owner, float radius, float damage)
    {
        gameObject.layer = owner == "Player" ? 16 : 15;
        tag = owner + "Bullet";
        sc.radius = radius;

        ps.vel.scale = 3 * sc.radius * ps.vel.scale.normalized;
        ps.properties.lifetime = sc.radius / ps.vel.scale.x;
        ps.size.val.z = -ps.size.val.y / ps.properties.lifetime;

        // light.range = p.explosionRadius*2;
        this.damage = damage;
    }

    private void hit(Vector3 pos)
    {
        Instantiate(hitPrefab, pos, Quaternion.identity);
    }

    private void OnTriggerEnter(Collider c)
    {
        //Debug.Log("BTrigger: " + c.name + " " + tag + " " + c.tag);
        EntityBase b = c.GetComponentInParent<EntityBase>();
        if (!b) {
            //Debug.LogWarning("Explosion hit non-Entity");
            return;
        }

        if (c.CompareTag("Enemy") && CompareTag("PlayerBullet"))
        {
            Vector3 distance = c.transform.position - transform.position;
            float distanceMultiplier = distance.magnitude > sc.radius/2 ? 3 : 6;
            //float damageMult = Mathf.SmoothStep(sc.radius, 0, (c.transform.position - transform.position).magnitude);
            Debug.Log("explosion hit Enemy" + damage);
            b.Hit(damage);
            b.KnockBack(distance.normalized * distanceMultiplier * 3f);
            hit(c.transform.position);
        }
        else if (c.CompareTag("Player") && CompareTag("EnemyBullet"))
        {
            float damageMult = Mathf.SmoothStep(sc.radius, 0, (c.transform.position - transform.position).magnitude);
            Debug.Log("explosion hit Player: " + "*" + damageMult);
            b.Hit(damage);
            hit(c.transform.position);
        }
    }
}
