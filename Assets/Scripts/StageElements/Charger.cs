using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Charger : MonoBehaviour
{
    private ChargeAnim anim;
    private Particles ps;
    private Renderer rend;
    private Color baseCol;
    public bool charging { get; private set; } = false;
    public bool charged { get; private set; } = false;

    private void Awake() {
        anim = GetComponentInChildren<ChargeAnim>();
        ps = GetComponent<Particles>();
        rend = GetComponent<Renderer>();
        baseCol = rend.material.color;
    }

    // Start is called before the first frame update
    void Start()
    {
        anim.ResetAnim();
    }

    public float dimLight = 1;
    private void Update()
    {
        dimLight += 3*(charged || anim.IsEnabled() ? Time.deltaTime : -Time.deltaTime);
        dimLight = Mathf.Clamp01(dimLight);

        Color c = baseCol;
        c.r = baseCol.r * 3*dimLight;
        rend.material.color = c;
    }

    public void SetEnabled(bool e) {
        anim.SetEnabled(false);
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
            if (!charging)
                foreach (EnemySpawner es in GameState.curStage.GetActorComponents<EnemySpawner>())
                    es.StartSpawning();
            charging = true;
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
        anim.ResetAnim(duration);
        ps.SetEnabled(false);
    }
}
