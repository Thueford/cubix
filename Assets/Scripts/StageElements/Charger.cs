using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Charger : MonoBehaviour
{
    private ChargeAnim anim;
    public bool charging { get; private set; } = false;
    public bool charged { get; private set; } = false;

    private void Awake() {
        anim = GetComponentInChildren<ChargeAnim>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Anim Start");
        anim.ResetAnim();
    }

    public void SetEnabled(bool e) { anim.SetEnabled(false); }

    public void OnChargeStart()
    {
        charging = true;
        foreach (EnemySpawner es in GameState.curStage.GetActorComponents<EnemySpawner>())
            es.StartSpawning();
    }

    public void OnCharged()
    {
        Debug.Log("Charged");
        charged = true;
        anim.SetEnabled(false);

        foreach (EnemySpawner es in GameState.curStage.GetActorComponents<EnemySpawner>())
            es.StopSpawning();
        GameState.curStage.portal.SetEnabled(true);
    }

    private void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("Player") && !GameState.curStage.portal.Enabled())
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
