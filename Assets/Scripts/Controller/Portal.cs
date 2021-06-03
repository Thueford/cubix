using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Disable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider c)
    {
        if(c.name.ToLower() == "player") {
            Debug.Log("Teleporting");
            Player.self.TeleportNext();
        }
    }

    public void Enable()
    {
        GetComponent<ParticleSystem>().Play();
        GetComponent<SphereCollider>().enabled = true;
    }

    public void Disable()
    {
        GetComponent<ParticleSystem>().Stop();
        GetComponent<SphereCollider>().enabled = false;
    }

    public bool Enabled()
    {
        return GetComponent<SphereCollider>().enabled;
    }
}
