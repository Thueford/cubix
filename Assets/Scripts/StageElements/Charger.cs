using UnityEngine;

[DisallowMultipleComponent]
public class Charger : MonoBehaviour
{
    private ChargeAnim anim;
    private Particles ps;
    private Renderer rend;
    private SphereCollider sc;

    private Color baseCol;
    private Dimmer dimLight = new Dimmer(3, 1, 0, 3);

    public bool active { get; private set; }
    public bool charging { get; private set; }
    public bool charged { get; private set; }

    public void EnableParticles(bool b = true) => ps.SetEnabled(b);
    public void SetChargeSpeed(float duration) => anim.SetAnimSpeed(duration);

    private void Awake()
    {
        anim = GetComponentInChildren<ChargeAnim>();
        ps = GetComponent<Particles>();
        rend = GetComponent<Renderer>();
        sc = GetComponent<SphereCollider>();
        baseCol = rend.material.color;
    }

    private void Update()
    {
        dimLight.Update(charged || anim.IsEnabled());
        rend.material.color = baseCol * dimLight.fRed;
    }

    public void SetEnabled(bool b)
    {
        Debug.Log("Charger.SetEnabled " + b);
        sc.enabled = b;
        anim.SetEnabled(false);
        EnemySpawner.EnableSpawning(GameState.curStage, b && active);
    }

    public void OnCharged()
    {
        Debug.Log("Charger.OnCharged");
        charged = true;
        ps.SetEnabled(true);
        SetEnabled(false);
        GameState.curStage.OnCharged();

        SoundHandler.PlayClip("chargeFinish");
    }

    private void OnTriggerEnter(Collider c)
    {
        if (!charged && c.CompareTag("Player"))
        {
            if (!active) EnemySpawner.EnableSpawning(GameState.curStage, active = true);
            if (!GameState.curStage.hasBoss()) 
            { 
                anim.SetEnabled(charging = true); 
                SoundHandler.PlayClip("chargeUp"); 
            }
        }
    }

    private void OnTriggerExit(Collider c)
    {
        if (!charged && c.CompareTag("Player"))
        {
            anim.SetEnabled(charging = false);
            if (!GameState.curStage.hasBoss())
                SoundHandler.PlayClip("chargeDown");
        }
    }

    internal void Reset(float duration = 0)
    {
        Debug.Log("Charger.Reset " + duration);
        active = false;
        charging = false;
        charged = false;

        if (duration > 0) anim.ResetAnim(duration);
        ps.SetEnabled(false);
        SetEnabled(true);
    }
}
