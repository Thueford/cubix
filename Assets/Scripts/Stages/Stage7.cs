using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage7 : StageController
{
    enum State { START, DONE }
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
}
