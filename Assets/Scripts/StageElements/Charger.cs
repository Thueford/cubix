using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Charger : MonoBehaviour
{
    private Portal portal;
    private ChargeAnim anim;
    public bool charging { get; private set; } = false;
    public bool charged { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Anim Start");
        anim = GetComponentInChildren<ChargeAnim>();
        anim.ResetAnim();
        portal = transform.parent.GetComponentInChildren<Portal>();
    }

    public void SetEnabled(bool e) { anim.SetEnabled(false); }

    public void OnChargeStart()
    {
        charging = true;
        foreach (EnemySpawner es in Player.curStage.actors.GetComponentsInChildren<EnemySpawner>())
            es.StartSpawning();
    }

    public void OnCharged()
    {
        Debug.Log("Charged");
        charged = true;
        anim.SetEnabled(false);

        foreach (EnemySpawner es in Player.curStage.actors.GetComponentsInChildren<EnemySpawner>())
            es.StopSpawning();
        portal.Enable();
    }

    private void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("Player") && !portal.Enabled())
            anim.SetEnabled(true);
    }

    private void OnTriggerExit(Collider c)
    {
        if (c.CompareTag("Player"))
            anim.SetEnabled(false);
    }

    internal void Reset(float duration)
    {
        charging = false;
        charged = false;
        anim.ResetAnim(duration);
    }
}
