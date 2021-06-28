using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class ChargeAnim : MonoBehaviour
{
    private Animator anim;
    private Particles ps;
    private float level;
    private const float maxScale = 0.24f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        ps = GetComponent<Particles>();
        ResetAnim();
    }

    private void Update()
    {
        level = transform.localScale.x / maxScale;
        if(level < 0.98) SetPsSize(level);
    }

    public void ResetAnim()
    {
        transform.localScale = new Vector3(0, 1, 0);
        SetEnabled(false);
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

    public void ResetAnim(float duration)
    {
        ResetAnim();
        anim.speed = 1 / duration;
    }

    void OnCharged(AnimationEvent ev) {
        transform.localScale = new Vector3(maxScale, 1, maxScale);
        SetPsSize(0);
        GetComponentInParent<Charger>().OnCharged();
    }

    void OnChargeStart(AnimationEvent ev) {
        GetComponentInParent<Charger>().OnChargeStart();
    }
}
