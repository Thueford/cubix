using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage4 : StageController
{
    enum State { START, WAIT, RED, GREEN, BLUE, DONE }
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
                Color color1 = GameState.colorOrder[0];

                foreach (Collectable c in GameState.curStage.GetActorComponents<Collectable>())
                {
                    if (color1.r > 0 && c.type == Collectable.cType.Red) Destroy(c.gameObject);
                    if (color1.g > 0 && c.type == Collectable.cType.Green) Destroy(c.gameObject);
                    if (color1.b > 0 && c.type == Collectable.cType.Blue) Destroy(c.gameObject);
                }
                state++;
                break;

            case State.WAIT:
                Color color2 = GameState.colorOrder[1];
                if (color2.r > 0) state = State.RED;
                if (color2.g > 0) state = State.GREEN;
                if (color2.b > 0) state = State.BLUE;
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
