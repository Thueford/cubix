using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Hint : MonoBehaviour
{
    [NotNull] public GameObject[] texts;

    virtual public void ResetHints()
    {
        foreach (GameObject o in texts)
            if(o != null) o.SetActive(false);
    }

    public bool isCurStage() {
        return GameState.curStage == GetComponentInParent<GameStage>();
    }
}
