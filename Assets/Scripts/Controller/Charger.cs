using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charger : MonoBehaviour
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
        Player.curStage.GetComponentInChildren<Portal>().Enable();
        GetComponentInChildren<Animator>().enabled = false;
    }

    private void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("Player"))
            GetComponentInChildren<Animator>().enabled = true;
    }

    private void OnTriggerExit(Collider c)
    {
        if (c.CompareTag("Player"))
            GetComponentInChildren<Animator>().enabled = false;
    }
}
