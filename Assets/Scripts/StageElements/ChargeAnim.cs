using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class ChargeAnim : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        ResetAnim();
    }

    public void ResetAnim()
    {
        anim.Play("Charging");
        transform.localScale = new Vector3(0, 1, 0);
        SetEnabled(false);
    }

    public void SetEnabled(bool b) { anim.enabled = b; }

    public void ResetAnim(float duration)
    {
        ResetAnim();
        anim.speed = 1 / duration;
    }

    void OnCharged(AnimationEvent ev) {
        GetComponentInParent<Charger>().OnCharged();
    }

    void OnChargeStart(AnimationEvent ev) {
        GetComponentInParent<Charger>().OnChargeStart();
    }
}
