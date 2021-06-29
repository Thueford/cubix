using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage9 : StageController
{
    enum State { START, DONE }
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
                texts[0].SetActive(true);
                state++;
                break;

            default: return;
        }
        
    }
}
