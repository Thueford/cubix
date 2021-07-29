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
    private float dimLight = 1;
    public bool charging { get; private set; }
    public bool charged { get; private set; }

    public void EnableParticles(bool b = true) => ps.SetEnabled(b);
    public void SetChargeSpeed(float duration) => anim.SetAnimSpeed(duration);

    private void Awake()
    {
        anim = GetComponentInChildren<ChargeAnim>();
        ps = GetComponent<Particles>();
        rend = GetComponent<Renderer>();
        baseCol = rend.material.color;
    }

    private void Update()
    {
        dimLight += 3*(charged || anim.IsEnabled() ? Time.deltaTime : -Time.deltaTime);
        dimLight = Mathf.Clamp01(dimLight);

        Color c = baseCol;
        c.r = baseCol.r * 3*dimLight;
        rend.material.color = c;
    }

    public void SetEnabled(bool b)
    {
        if (!charging) return;
        Debug.Log("Charger.SetEnabled " + b);
        anim.SetEnabled(b);
        EnemySpawner.EnableSpawning(GameState.curStage, b);
    }

    public void OnCharged()
    {
        Debug.Log("Charger.OnCharged");
        charged = true;
        ps.SetEnabled(true);
        SetEnabled(false);
        GameState.curStage.OnCharged();
    }

    private void OnTriggerEnter(Collider c)
    {
        if (!charged && c.CompareTag("Player"))
        {
            if (!charging)
            {
                charging = true;
                SetEnabled(true);
            }
            else anim.SetEnabled(true);
        }
    }

    private void OnTriggerExit(Collider c)
    {
        if (!charged && c.CompareTag("Player"))
            anim.SetEnabled(false);
    }

    internal void Reset(float duration = 0)
    {
        Debug.Log("Charger.Reset " + duration);
        charging = false;
        charged = false;
        anim.ResetAnim(duration);
        ps.SetEnabled(false);
    }
}
