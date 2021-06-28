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
        SetPsSize(1.2f * level);
    }

    public void ResetAnim()
    {
        transform.localScale = new Vector3(0, 1, 0);
        SetEnabled(false);
    }

    private void SetPsSize(float v)
    {
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
        transform.localScale = new Vector3(maxScale, 1, maxScale);
        GetComponentInParent<Charger>().OnCharged();
    }

    void OnChargeStart(AnimationEvent ev) {
        GetComponentInParent<Charger>().OnChargeStart();
    }
}
