using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [NotNull] public SphereCollider sc;
    [NotNull] public Particles.Particles ps;
    private float lifespan = 0.1f, age = 0, damage = 0;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("StopExplosion", 0.6f);
        Invoke("StopCollision", lifespan);
    }

    void StopExplosion() { Destroy(gameObject); }
    void StopCollision() { sc.enabled = false; }

    public void SetProperties(Bullet.Properties p)
    {
        gameObject.layer = p.owner == "Player" ? 16 : 15;
        tag = tag = p.owner + "Bullet";
        sc.radius = p.explosionRadius;
        ps.vel.scale = 2 * ps.vel.scale.normalized * p.explosionRadius / ps.properties.lifetime;
        // light.range = p.explosionRadius*2;
        damage = p.damage;
    }

    private void OnTriggerEnter(Collider c)
    {
        //Debug.Log("BTrigger: " + c.name + " " + tag + " " + c.tag);
        if (c.CompareTag("Enemy") && CompareTag("PlayerBullet"))
        {
            EntityBase b = c.GetComponentInParent<EntityBase>();
            if (b)
            {
                //float damageMult = Mathf.SmoothStep(sc.radius, 0, (c.transform.position - transform.position).magnitude);
                Debug.Log("explosion hit Enemy" + damage);
                b.Hit(damage);
            }
            else Debug.LogWarning("Explosion hit non-Entity");
        }
        else if (c.CompareTag("Player") && CompareTag("EnemyBullet"))
        {
            EntityBase b = c.GetComponentInParent<EntityBase>();
            if (b)
            {
                float damageMult = Mathf.SmoothStep(sc.radius, 0, (c.transform.position - transform.position).magnitude);
                Debug.Log("explosion hit Player");
                b.Hit(damage);
            }
            else Debug.LogWarning("explosion hit non-Entity");
        }
    }
}
