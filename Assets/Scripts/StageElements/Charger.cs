using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Charger : MonoBehaviour
{
    private ChargeAnim anim;
    private Particles ps;
    public bool charging { get; private set; } = false;
    public bool charged { get; private set; } = false;

    private void Awake() {
        anim = GetComponentInChildren<ChargeAnim>();
        ps = GetComponent<Particles>();
    }

    // Start is called before the first frame update
    void Start()
    {
        anim.ResetAnim();
    }

    public void SetEnabled(bool e) {
        anim.SetEnabled(false);
    }

    public void OnChargeStart()
    {
        charging = true;
        foreach (EnemySpawner es in GameState.curStage.GetActorComponents<EnemySpawner>())
            es.StartSpawning();
    }

    public void OnCharged()
    {
        charged = true;
        anim.SetEnabled(false);
        ps.SetEnabled(true);

        foreach (EnemySpawner es in GameState.curStage.GetActorComponents<EnemySpawner>())
            es.StopSpawning();
        GameState.curStage.portal.SetEnabled(true);
    }

    private void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("Player") && !GameState.curStage.portal.Enabled())
        {
            //foreach (EnemySpawner es in GameState.curStage.GetActorComponents<EnemySpawner>())
            //    if (!es.isSpawning) es.StartSpawning();
            anim.SetEnabled(true);
        }
    }

    private void OnTriggerExit(Collider c)
    {
        if (c.CompareTag("Player"))
            anim.SetEnabled(false);
    }

    internal void Reset(float duration = 0)
    {
        charging = false;
        charged = false;
        if (anim)
            anim.ResetAnim(duration);
        if (ps)
            ps.SetEnabled(false);
    }
}
