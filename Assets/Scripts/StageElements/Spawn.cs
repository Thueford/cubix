using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Spawn : MonoBehaviour
{
    private Particles ps;

    private void Awake()
    {
        ps = GetComponent<Particles>();
    }

    public void SetEnabled(bool b)
    {
        if (b) ps.ResetPS();
        ps.SetEnabled(b);
    }
}
