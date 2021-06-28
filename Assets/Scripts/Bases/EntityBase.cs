using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public abstract class EntityBase : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(1, 50)]
    public float accelerationForce = 15;
    [Range(1, 50)]
    public float maxSpeed = 10;
    public bool movable = false;

    [Header("HP Settings")]
    [Range(1, 100)] public float startHP = 5;
    [Range(1, 100)] public float maxHP = 5;
    [ReadOnly] public float HP;

    public Vector3Int rgb = Vector3Int.zero;

    // [Header("Other Settings")]

    protected Animator anim;
    protected Rigidbody rb;

    virtual public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    virtual public void Start()
    {
        HP = startHP;
        anim.keepAnimatorControllerStateOnDisable = true;
        Freeze();
    }

    virtual public void Update() {}
    virtual public void FixedUpdate() {}

    public void setColor(Color c)
    {
        rgb = Vector3Int.FloorToInt((Vector4)c);
        GetComponentInChildren<Renderer>().material.color = c;
        GetComponentInChildren<Light>().color = GameState.getLightColor(c);
    }

    virtual public void Hit(float damage)
    {
        HP -= damage;
        if (HP <= 0) Die();
    }

    virtual public void Freeze()
    {
        movable = false;
        foreach (ShooterBase sb in GetComponents<ShooterBase>()) sb.active = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    virtual public void Melt()
    {
        movable = true;
        foreach (ShooterBase sb in GetComponents<ShooterBase>()) sb.active = true;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = true;
    }

    virtual public void Die()
    {
        Debug.Log("killing " + name);
        gameObject.GetComponentInChildren<Light>().enabled = false;
        anim.enabled = true;
        anim.Play("Die");
        Freeze();
    }

    virtual public void OnDie(AnimationEvent ev)
    {
        anim.enabled = false;
        // Debug.Log("killed " + name);
    }

    virtual public void OnSpawn(AnimationEvent ev)
    {
        Melt();
        anim.enabled = false;
    }

    virtual public void KnockBack(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

    /*
    virtual public void killStuckAnim()
    {
        if (anim.enabled)
        {
            Destroy(gameObject);
        }
    }
    */
}
