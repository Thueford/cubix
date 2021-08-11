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
    public int targetNo;
    public bool isEndless = false;
    private bool active;

    private void Awake()
    {
        sc = GetComponent<SphereCollider>();
        ps = GetComponent<Particles>();
        r = GetComponent<Renderer>();
        l = GetComponent<Light>();
        if (color.a == 0) color = r.material.color;
    }

    private void Start()
    {
        SetColor(color);
        if (isEndless)
        {
            if (this != GetComponentInParent<GameStage>().portal) 
                SetEnabled(true);
            ps.color.color2 = 0.2f * Color.red + 0.8f * Color.black;
            r.material.SetColor("_EmissionColor", ps.color.color2);
        }
    }
    public virtual void Update()
    {
        dimCol.Update(active);
        l.color = color * dimCol.fCol;
        r.material.SetColor("_EmissionColor", color * dimCol.fCol);
    }

    private void OnTriggerEnter(Collider c)
    {
        if(c.CompareTag("Player"))
        {
            Debug.Log("Teleporting");
            if (isEndless)
            {
                InputHandler.enableSpace = true;
                InputHandler.enableNumbers = true;
                GameState.addRed();
                GameState.addGreen();
                GameState.addBlue();
            }
            Player.self.TeleportNext(target, targetNo);
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
