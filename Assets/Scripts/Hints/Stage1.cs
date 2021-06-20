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
                if (isCurStage()) return;
                texts[0].SetActive(true);
                state++;
                break;

            case State.CHARGING:
                if (!GameState.curStage.charger.charging) return;
                texts[1].SetActive(true);
                state++;
                break;
            default: return;
        }
        
    }
}
