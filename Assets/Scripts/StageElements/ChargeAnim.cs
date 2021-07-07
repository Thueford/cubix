using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class ChargeAnim : MonoBehaviour
{
    private Animator anim;
    private Particles ps;
    private Charger charger;
    private float level;
    private const float maxScale = 0.24f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        ps = GetComponent<Particles>();
        charger = transform.parent.GetComponent<Charger>();
        ResetAnim();
    }

    private void Update()
    {
        level = transform.localScale.x / maxScale;
        if(level < 0.98) SetPsSize(level);
    }

    private void SetPsSize(float v)
    {
        const float f = 1.2f;
        ps.pos.offset.x = v * f;
        ps.pos.offset.z = v * f;
        ps.pos.scale.x = -v * f / 2;
        ps.pos.scale.z = -v * f / 2;
        ps.properties.emissionRate = v * ps.properties.maxParts;
    }

    public void SetEnabled(bool b) { anim.enabled = b; }

    public void ResetAnim(float duration = -1)
    {
        if (duration > 0) anim.speed = 1 / duration;
        transform.localScale = new Vector3(0, 1, 0);
        anim.Play("Charging", 0, 0);
        SetEnabled(false);
    }

    void OnCharged(AnimationEvent ev) {
        transform.localScale = new Vector3(maxScale, 1, maxScale);
        SetPsSize(0);
        charger.OnCharged();
    }

    void OnChargeStart(AnimationEvent ev) {
        charger.OnChargeStart();
    }
}
