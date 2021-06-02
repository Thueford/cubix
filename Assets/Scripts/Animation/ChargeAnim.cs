using UnityEngine;

public class ChargeAnim : MonoBehaviour
{
    void Start()
    {
        Reset();
    }

    public void Reset()
    {
        Debug.Log("AnimReset");
        GetComponent<Animator>().Play("Charging");
        GetComponent<Animator>().enabled = false;
        transform.localScale = new Vector3(0, 1, 0);
    }

    void OnCharged(AnimationEvent ev) {
        GetComponentInParent<Charger>().OnCharged(ev);
    }
}
