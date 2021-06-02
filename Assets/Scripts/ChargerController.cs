using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargerController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCharged(AnimationEvent ev)
    {
        Debug.Log("Charged");
    }

    private void OnTriggerEnter(Collider c)
    {
        Debug.Log("CollEn: " + c.gameObject.name);
        if (c.gameObject.name.ToLower() == "player")
            GetComponentInChildren<Animator>().enabled = true;
    }

    private void OnTriggerExit(Collider c)
    {
        Debug.Log("CollEx: " + c.gameObject.name);
        if (c.gameObject.name.ToLower() == "player")
            GetComponentInChildren<Animator>().enabled = false;
    }
}
