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
    }

    private void Update()
    {
        if (level < 0) transform.localScale = new Vector3(0, 1, 0);
        level = transform.localScale.x / maxScale;
        if (level < 0.98) SetPsSize(level);
    }

    private void SetPsSize(float v)
    {
        const float f = 1.5f;
        ps.pos.offset.x = v * f;
        ps.pos.offset.z = v * f;
        ps.pos.scale.x = -v * f / 2;
        ps.pos.scale.z = -v * f / 2;
        ps.properties.emissionRate = v*v * ps.properties.maxParts;
    }

    public void SetEnabled(bool b)
    {
        Debug.Log("ChargeAnim.SetEnabled " + b);
        anim.enabled = b;
    }

    public void SetAnimSpeed(float duration) => anim.speed = 1 / duration;
    public bool IsEnabled() => anim.enabled;

    public void ResetAnim(float duration = 0)
    {
        SetAnimSpeed(duration);
        anim.Play("Charging", 0, 0);
        // SetEnabled(false);
        level = -1;
    }

    void OnCharged(AnimationEvent ev)
    {
        transform.localScale = new Vector3(maxScale, 1, maxScale);
        SetPsSize(0);
        charger.OnCharged();
    }
}
