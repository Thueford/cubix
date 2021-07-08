using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SphereCollider))]
public class Portal : MonoBehaviour
{
    private SphereCollider sc;
    private Particles ps;
    public GameStage target;
    public bool isEndless = false;
    
    private void Awake()
    {
        sc = GetComponent<SphereCollider>();
        ps = GetComponent<Particles>();
    }

    private void Start()
    {
        if (isEndless)
        {
            Renderer r = GetComponent<Renderer>();
            ps.color.color = Color.red;
            ps.color.color2 = 0.2f * Color.red + 0.8f * Color.black;

            r.material.color = ps.color.color;
            r.material.SetColor("_EmissionColor", ps.color.color2);
        }
    }

    private void OnTriggerEnter(Collider c)
    {
        if(c.CompareTag("Player"))
        {
            Debug.Log("Teleporting");
            Player.self.TeleportNext(target);
        }
    }

    public void SetEnabled(bool b)
    {
        ps.SetEnabled(b);
        sc.enabled = b;
    }

    public bool Enabled() { return sc.enabled; }
}
