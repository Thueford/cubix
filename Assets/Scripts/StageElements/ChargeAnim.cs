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
        if(level < .95) SetPsSize(1.2f * level);
    }

    public void ResetAnim()
    {
        anim.Play("Charging");
        transform.localScale = new Vector3(0, 1, 0);
        SetEnabled(false);
    }

    private void SetPsSize(float v)
    {
        v = 1.2f * v;
        ps.pos.offset.x = v;
        ps.pos.offset.z = v;
        ps.pos.scale.x = -v / 2;
        ps.pos.scale.z = -v / 2;
        ps.properties.emissionRate = level * ps.properties.maxParts;
    }

    public void SetEnabled(bool b) { anim.enabled = b; }

    public void ResetAnim(float duration)
    {
        ResetAnim();
        anim.speed = 1 / duration;
    }

    void OnCharged(AnimationEvent ev) {
        SetPsSize(5);
        transform.localScale = new Vector3(maxScale, 1, maxScale);
        GetComponentInParent<Charger>().OnCharged();
    }

    void OnChargeStart(AnimationEvent ev) {
        // SetPsSize(0);
        GetComponentInParent<Charger>().OnChargeStart();
    }
}
