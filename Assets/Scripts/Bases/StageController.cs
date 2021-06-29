using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class StageController : MonoBehaviour
{
    [NotNull] public GameObject[] texts;

    virtual public void ResetHints()
    {
        foreach (GameObject o in texts)
            if(o != null) o.SetActive(false);
    }

    private void Update()
    {
        if (!isCurStage()) return;
        if (GameState.settings.reachedEndless) EndlessHints(); 
        else NewbieHints();
    }

    public bool isCurStage() {
        return GameState.curStage == GetComponentInParent<GameStage>();
    }

    public abstract void NewbieHints();

    public virtual void EndlessHints() { }
}
