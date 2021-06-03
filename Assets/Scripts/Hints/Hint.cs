using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hint : MonoBehaviour
{
    public GameObject[] texts;

    // Start is called before the first frame update
    void Start()
    {
        // for (int i = 1; i < texts.Length; i++) texts[i].SetActive(false);
        foreach (GameObject o in texts) o.SetActive(false);
    }
}
