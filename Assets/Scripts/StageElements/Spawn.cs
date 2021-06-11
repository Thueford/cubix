using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Spawn : MonoBehaviour
{
    public void Enable()
    {
        GetComponentInChildren<ParticleSystem>().Play();
    }

    public void Disable()
    {
        GetComponentInChildren<ParticleSystem>().Stop();
    }
}