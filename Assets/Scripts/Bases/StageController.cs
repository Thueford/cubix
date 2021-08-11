using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class StageController : MonoBehaviour
{
    [NotNull] public GameObject[] texts;
    private GameStage parentStage;

    private void Start()
    {
        parentStage = GetComponentInParent<GameStage>();
    }

    virtual public void ResetHints()
    {
        CancelInvoke();
        foreach (GameObject o in texts)
            if(o != null) o.SetActive(false);
    }

    private void Update()
    {
        if (!isCurStage()) return;
        if (PlayerStats.self.reachedEndless) Experienced(); 
        if (!PlayerStats.self.reachedEndless || GameState.IsTutorial()) Newbie();
        General();
    }

    public bool isCurStage() => GameState.curStage == parentStage;

    public virtual void Newbie() { }

    public virtual void Experienced() { }

    public virtual void General() { }
}
