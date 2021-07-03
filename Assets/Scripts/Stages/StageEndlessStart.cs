using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEndlessStart : StageController
{
    enum State { START, DONE }
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
                texts[0].SetActive(true);
                InputHandler.enableSpace = true;
                InputHandler.enableNumbers = true;
                if (GameState.unlockedColors.x == 0) GameState.addRed();
                if (GameState.unlockedColors.y == 0) GameState.addGreen();
                if (GameState.unlockedColors.z == 0) GameState.addBlue();
                state++;
                break;
        }
    }
}
