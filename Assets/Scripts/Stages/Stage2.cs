using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage2 : StageController
{
    enum State { START, RED, GREEN, BLUE, DONE }
    private State state;

    override public void ResetHints()
    {
        base.ResetHints();
        state = State.START;
    }

    override public void General()
    {
        switch (state)
        {
            case State.START:
                if (GameState.unlockedColors == Vector3Int.right) state = State.RED;
                else if (GameState.unlockedColors == Vector3Int.up) state = State.GREEN;
                else if (GameState.unlockedColors == Vector3Int.forward) state = State.BLUE;
                break;

            case State.RED:
            case State.GREEN:
            case State.BLUE:
                Collectable.Clear(GameState.curStage);
                state = State.DONE;
                break;
        }
    }

    override public void Newbie()
    {
        switch (state)
        {
            case State.START: texts[0].SetActive(true); break;
            case State.RED  : texts[1].SetActive(true); break;
            case State.GREEN: texts[2].SetActive(true); break;
            case State.BLUE : texts[3].SetActive(true); break;
        }
    }
}
