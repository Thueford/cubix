using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charger : MonoBehaviour
{
    private Portal portal;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        portal = transform.parent.GetComponentInChildren<Portal>();
        anim = GetComponentInChildren<Animator>();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnCharged(AnimationEvent ev)
    {
        Debug.Log("Charged");
        portal.Enable();
        anim.enabled = false;
    }

    private void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("Player") && !portal.Enabled())
            anim.enabled = true;
    }

    private void OnTriggerExit(Collider c)
    {
        if (c.CompareTag("Player"))
            anim.enabled = false;
    }
}
