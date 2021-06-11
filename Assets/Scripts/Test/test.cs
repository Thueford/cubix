using System.Collections;
using UnityEngine;

public class test : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    private void OnLoad()
    {
        Debug.Log("Hello WOrld");
    }
}