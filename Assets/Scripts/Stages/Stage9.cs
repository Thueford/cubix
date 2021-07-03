using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage9 : StageController
{
    enum State { START, CONGRATS, DONE }
    private State state;

    override public void ResetHints()
    {
        base.ResetHints();
        state = State.START;
    }

    override public void Newbie()
    {
        switch (state)
        {
            case State.START:
                texts[0].SetActive(true);
                state++;
                break;
        }
    }

    override public void General()
    {
        switch (state)
        {
            case State.CONGRATS:
                if (!GameState.curStage.charger.charged) return;
                texts[1].SetActive(true);
                state++;
                break;
        }
    }
}
