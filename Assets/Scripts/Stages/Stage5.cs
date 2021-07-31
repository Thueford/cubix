using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage5 : StageController
{
    enum State { START, SPACE, DONE }
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
                InputHandler.enableSpace = true;
                texts[0].SetActive(true);
                state++;
                break;
            case State.SPACE:
                if (!InputHandler.ReadSpaceInput()) return;
                texts[1].SetActive(true);
                state++;
                break;
        }
    }
}
