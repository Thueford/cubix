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

    virtual public void Awake() {}

    // Start is called before the first frame update
    virtual public void Start()
    {
        HP = startHP;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        anim.keepAnimatorControllerStateOnDisable = true;
        Freeze();
    }

    virtual public void Update() {}
    virtual public void FixedUpdate() {}

    public void setColor(Color c)
    {
        rgb.x = (int)c.r;
        rgb.y = (int)c.g;
        rgb.z = (int)c.b;
        GetComponentInChildren<Renderer>().material.color = c;
        GetComponentInChildren<Light>().color = c == Color.black ? GameState.glow : c;
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
        anim.enabled = true;
        anim.Play("Die");
        Freeze();
    }

    virtual public void OnDie(AnimationEvent ev)
    {
        Debug.Log("killed " + name);
        if (rgb.x == 1) Ressource.self.addRes(Ressource.col.Red, 10);
        if (rgb.y == 1) Ressource.self.addRes(Ressource.col.Green, 10);
        if (rgb.z == 1) Ressource.self.addRes(Ressource.col.Blue, 10);
    }

    virtual public void OnSpawn(AnimationEvent ev)
    {
        Melt();
        anim.enabled = false;
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
