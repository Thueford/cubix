using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage1 : Hint
{
    enum State { START,  CHARGING, DONE }
    private State state;

    override public void ResetHints()
    {
        base.ResetHints();
        state = State.START;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.START:
                if (Player.curStage != GetComponentInParent<GameStage>()) return;
                texts[0].SetActive(true);
                state++;
                break;

            case State.CHARGING:
                if (Player.curStage != GetComponentInParent<GameStage>()) return;
                //if (!Player.curStage.GetComponentInChildren<Charger>().charging) return;
                texts[1].SetActive(true);
                state++;
                break;
            default: return;
        }
        
    }
}
