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
    [Range(1, 100)]
    public float startHP = 5;
    [ReadOnly] public float HP;

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
        Freeze();
    }

    virtual public void Update() {}
    virtual public void FixedUpdate() {}

    public void setColor(Color c)
    {
        GetComponentInChildren<Renderer>().material.color = c;
        GetComponentInChildren<Light>().color = c == Color.black ? GameState.glow : c;
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
        if (this is EnemyBase) EnemySpawner.EnemyDied();
    }

    virtual public void OnDie(AnimationEvent ev)
    {
        Debug.Log("killed " + name);
        if (!(this is Player)) Destroy(gameObject);
    }

    virtual public void OnSpawn(AnimationEvent ev)
    {
        Melt();
        anim.enabled = false;
        if (this is EnemyBase) EnemySpawner.EnemySpawned();
    }
}
