using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage3 : Hint
{
    enum State { START, DONE }
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

            default: return;
        }
        
    }
}
