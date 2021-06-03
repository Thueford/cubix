using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBase : MonoBehaviour
{
    public float accelerationForce;
    public float maxSpeed;
    public bool movable;
    public float HP;

    protected Animator anim;
    protected Rigidbody rb;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    virtual protected void Update()
    {

    }

    public void Hit(float damage)
    {
        HP -= damage;
        if (HP <= 0) Die();
    }

    public void Die()
    {
        Debug.Log("killing " + name);
        anim.enabled = true;
        anim.Play("Die");
    }

    public void OnDie(AnimationEvent ev)
    {
        Debug.Log("killed " + name);
        if (this is EnemyBase) Destroy(gameObject);
    }
}
