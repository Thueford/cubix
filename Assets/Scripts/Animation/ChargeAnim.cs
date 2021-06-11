using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ChargeAnim : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        Reset();
    }

    public void Reset()
    {
        anim.Play("Charging");
        anim.enabled = false;
        transform.localScale = new Vector3(0, 1, 0);
    }

    public void Reset(float time)
    {
        Reset();
        anim.speed = 1/time;
    }

    void OnCharged(AnimationEvent ev) {
        GetComponentInParent<Charger>().OnCharged(ev);
    }
}
