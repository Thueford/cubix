using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SphereCollider))]
public class Portal : MonoBehaviour
{
    private SphereCollider sc;
    private Particles ps;

    private void Awake()
    {
        sc = GetComponent<SphereCollider>();
        ps = GetComponent<Particles>();
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
        ps.SetEnabled(b);
        sc.enabled = b;
    }

    public bool Enabled() { return sc.enabled; }
}
