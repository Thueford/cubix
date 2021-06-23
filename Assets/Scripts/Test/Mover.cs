using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public float timeScale = 4, radius = 5;
    void Update()
    {
        Vector3 f = radius * new Vector3(Mathf.Cos(timeScale * Time.time), 0, Mathf.Sin(timeScale * Time.time));
        transform.position += f * Time.deltaTime;
    }
}
