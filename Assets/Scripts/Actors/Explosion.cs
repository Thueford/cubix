using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [NotNull] public SphereCollider sc;
    [NotNull] public Particles ps;
    [NotNull] public GameObject hitPrefab;
    private float lifespan = 0.1f, damage = 0;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("StopCollision", lifespan);
    }

    void Update()
    {
        if (damage >= 10 && Time.timeScale < 1)
            Time.timeScale = Mathf.Clamp01(Time.timeScale + Time.deltaTime / 6);
    }

    // destroyed by ParticleSystem
    // void StopExplosion() { Destroy(gameObject); }
    void StopCollision() { sc.enabled = false; }

    public void SetProperties(Bullet.Properties p)
    {
        SetProperties(p.owner, p.explosionRadius, p.damage);
    }

    public void SetProperties(string owner, float radius, float damage, float velScale = 3)
    {
        gameObject.layer = owner == "Player" ? 16 : 15;
        tag = owner + "Bullet";
        sc.radius = radius;

        ps.vel.scale = velScale * sc.radius * ps.vel.scale.normalized;
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
        EntityBase b = c.GetComponentInParent<EntityBase>();
        if (!b) {
            Debug.LogWarning($"Explosion hit non-Entity ({c.name})");
            return;
        }

        if (c.CompareTag("Enemy") && CompareTag("PlayerBullet"))
        {
            Vector3 distance = b.pos - transform.position;
            float distanceMultiplier = distance.magnitude > sc.radius/2 ? 3 : 6;
            //float damageMult = Mathf.SmoothStep(sc.radius, 0, (b.pos - transform.position).magnitude);
            b.Hit(damage);
            b.KnockBack(distance.normalized * distanceMultiplier * 3f);
            hit(b.pos);
        }
        else if (c.CompareTag("Player") && CompareTag("EnemyBullet"))
        {
            float damageMult = Mathf.SmoothStep(sc.radius, 0, (b.pos - transform.position).magnitude);
            b.Hit(damage);
            hit(b.pos);
        }
    }
}
