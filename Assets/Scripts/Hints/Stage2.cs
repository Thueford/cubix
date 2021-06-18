using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage2 : Hint
{
    enum State { START, RED, GREEN, BLUE, DONE }
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
                if (GameState.unlockedColors == Vector3Int.right) state = State.RED;
                else if (GameState.unlockedColors == Vector3Int.up) state = State.GREEN;
                else if (GameState.unlockedColors == Vector3Int.forward) state = State.BLUE;
                break;

            case State.RED:
                if (Player.curStage != GetComponentInParent<GameStage>()) return;
                texts[1].SetActive(true);
                state = State.DONE;
                break;

            case State.GREEN:
                if (Player.curStage != GetComponentInParent<GameStage>()) return;
                texts[2].SetActive(true);
                state = State.DONE;
                break;

            case State.BLUE:
                if (Player.curStage != GetComponentInParent<GameStage>()) return;
                texts[3].SetActive(true);
                state = State.DONE;
                break;

            default: return;
        }
        
    }
}
