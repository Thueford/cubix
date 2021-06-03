using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hint : MonoBehaviour
{
    public GameObject[] texts;

    virtual public void ResetHints()
    {
        foreach (GameObject o in texts) o.SetActive(false);
    }
}
