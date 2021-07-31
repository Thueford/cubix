using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SphereCollider))]
public class Portal : MonoBehaviour
{
    private SphereCollider sc;
    private Particles ps;
    private Light l;
    private Renderer r;

    public Color color;
    private Dimmer dimCol = new Dimmer(1f, 0, 0.1f, 1);

    public GameStage target;
    public bool isEndless = false;
    private bool active;

    private void Awake()
    {
        sc = GetComponent<SphereCollider>();
        ps = GetComponent<Particles>();
        r = GetComponent<Renderer>();
        l = GetComponent<Light>();
    }

    private void Start()
    {
        if (isEndless)
        {
            SetColor(color = Color.red);
            ps.color.color2 = 0.2f * Color.red + 0.8f * Color.black;
            r.material.SetColor("_EmissionColor", ps.color.color2);
        }
    }
    public virtual void Update()
    {
        dimCol.Update(active);
        l.color = color * dimCol.fCol;
        ps.properties.emissionRate = 30 * dimCol;
        r.material.SetColor("_EmissionColor", color * dimCol.fCol);
    }

    private void OnTriggerEnter(Collider c)
    {
        if(c.CompareTag("Player"))
        {
            Debug.Log("Teleporting");
            Player.self.TeleportNext(target);
        }
    }

    public void SetColor(Color c)
    {
        l.color = c;
        ps.color.color = c;

        r.material.color = c;
        r.material.SetColor("_EmissionColor", c);
    }

    public void SetEnabled(bool b)
    {
        ps.SetEnabled(b);
        sc.enabled = active = b;
    }
}
