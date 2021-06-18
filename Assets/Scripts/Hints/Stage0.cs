using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage0 : Hint
{
    enum State { START, WASD, CHARGE, PORTAL }
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
                texts[0].SetActive(true);
                break;

            case State.WASD:
                if (!(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) ||
                    Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) ||
                    Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) ||
                    Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))) return;
                texts[1].SetActive(true);
                break;

            case State.CHARGE:
                if (!Player.curStage.portal.Enabled()) return;
                texts[2].SetActive(true);
                break;

            default: return;
        }
        state++;
    }
}
