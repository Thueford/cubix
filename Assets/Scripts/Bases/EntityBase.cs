using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBase : MonoBehaviour
{
    protected float accelerationForce;
    protected float maxSpeed;
    protected bool movable;
    protected float HP;

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
        Debug.Log("Hit: " + name);
        HP -= damage;
        if (HP <= 0) GetComponent<Animator>().Play("E_Die");
    }

    public void OnDie(AnimationEvent ev)
    {
        Debug.Log("killed " + name);
        Destroy(gameObject);
    }
}