using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SphereCollider))]
public class Portal : MonoBehaviour
{
    SphereCollider sc;
    ParticleSystem ps;

    private void Start()
    {
        sc = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider c)
    {
        if(c.CompareTag("Player"))
        {
            Debug.Log("Teleporting");
            Player.self.TeleportNext();
        }
    }

    public void SetEnabled(bool b)
    {
        if (b) ps.Play(); 
        else ps.Stop();
        sc.enabled = b;
    }

    public bool Enabled() { return sc.enabled; }
}
