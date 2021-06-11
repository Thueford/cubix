using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hint : MonoBehaviour
{
    [NotNull] public GameObject[] texts;

    virtual public void ResetHints()
    {
        foreach (GameObject o in texts)
            if(o != null) o.SetActive(false);
    }
}
