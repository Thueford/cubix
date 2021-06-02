using UnityEngine;

public class ChargingController : MonoBehaviour
{
    private void Start()
    {
        transform.localScale = new Vector3(0,1,0);
        GetComponent<Animator>().enabled = false;
    }

    void OnCharged(AnimationEvent ev) {
        GetComponentInParent<Charger>().OnCharged(ev);
    }
}
