using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SphereCollider))]
public class Portal : MonoBehaviour
{
    [NotNull] public SphereCollider sc;
    [NotNull] public Particles ps;
    public GameStage target;

    
    private void Awake()
    {
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
