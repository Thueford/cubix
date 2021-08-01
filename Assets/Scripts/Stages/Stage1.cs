using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage1 : StageController
{
    enum State { START, MOVING, CHARGING, DONE }
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
                break;

            case State.MOVING:
                if (!Player.self.movable || InputHandler.ReadDirInput() == Vector3.zero) return;
                texts[1].SetActive(true);
                ++state;
                break;

            case State.CHARGING:
                if (!GameState.curStage.charger.active) return;
                texts[2].SetActive(true);
                ++state;
                break;
        }
    }

    public override void General()
    {
        base.General();
        switch (state)
        {
            case State.START:
                ++state;
                break;
            default: break;
        }
    }
}
