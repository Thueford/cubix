using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityBase : MonoBehaviour
{
    public float accelerationForce;
    public float maxSpeed;
    public bool movable;
    public float HP;

    protected Animator anim;
    protected Rigidbody rb;

    virtual public void Awake()
    {

    }

    // Start is called before the first frame update
    virtual public void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        Freeze();
    }

    virtual public void Update()
    {

    }

    public void Hit(float damage)
    {
        HP -= damage;
        if (HP <= 0) Die();
    }

    virtual public void Freeze()
    {
        movable = false;
        foreach (ShooterBase sb in GetComponents<ShooterBase>()) sb.active = false;
    }

    virtual public void Melt()
    {
        movable = true;
        foreach (ShooterBase sb in GetComponents<ShooterBase>()) sb.active = true;
    }

    virtual public void Die()
    {
        Debug.Log("killing " + name);
        anim.enabled = true;
        anim.Play("Die");
        Freeze();
        if (this is Player) Player.curStage.OnStageExit();
    }

    virtual public void OnDie(AnimationEvent ev)
    {
        Debug.Log("killed " + name);
        if (!(this is Player)) Destroy(gameObject);
    }

    virtual public void OnSpawn(AnimationEvent ev)
    {
        anim.enabled = false;
    }
}
