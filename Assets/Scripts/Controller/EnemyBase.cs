using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public float accelerationForce = 10f;
    public float maxSpeed = 10f;
    public bool movable = true;

    public Animator anim;
    public Rigidbody rb;

    // Start is called before the first frame update
    virtual public void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    virtual public void Update()
    {
        
    }
}
