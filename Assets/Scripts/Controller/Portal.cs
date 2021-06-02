using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider c)
    {
        if(c.name.ToLower() == "player") {
            Debug.Log("Teleporting");
            Player.TeleportNext();
        }
    }
}
