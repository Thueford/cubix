using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage1 : StageController
{
    enum State { START, CHARGING, DONE }
    private State state;

    override public void ResetHints()
    {
        base.ResetHints();
        state = State.START;
    }

    override public void NewbieHints()
    {
        switch (state)
        {
            case State.START:
                InputHandler.enableSpace = false;
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
