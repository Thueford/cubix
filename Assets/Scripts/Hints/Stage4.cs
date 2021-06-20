using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage4 : Hint
{
    enum State { START, WAIT, RED, GREEN, BLUE, DONE }
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
                Color color1 = GameState.colorOrder[0];

                foreach (Collectable c in Player.curStage.actors.GetComponentsInChildren<Collectable>())
                {
                    if (color1.r > 0 && c.type == Collectable.cType.Red) Destroy(c.gameObject);
                    if (color1.g > 0 && c.type == Collectable.cType.Green) Destroy(c.gameObject);
                    if (color1.b > 0 && c.type == Collectable.cType.Blue) Destroy(c.gameObject);
                }
                texts[0].SetActive(true);
                state++;
                break;

            case State.WAIT:
                if (Player.curStage != GetComponentInParent<GameStage>()) return;
                Color color2 = GameState.colorOrder[1];
                if (color2.r > 0) state = State.RED;
                if (color2.g > 0) state = State.GREEN;
                if (color2.b > 0) state = State.BLUE;
                break;

            case State.RED:
                if (Player.curStage != GetComponentInParent<GameStage>()) return;
                texts[1].SetActive(true);
                Collectable.Clear(Player.curStage);
                state = State.DONE;
                break;

            case State.GREEN:
                if (Player.curStage != GetComponentInParent<GameStage>()) return;
                texts[2].SetActive(true);
                Collectable.Clear(Player.curStage);
                state = State.DONE;
                break;

            case State.BLUE:
                if (Player.curStage != GetComponentInParent<GameStage>()) return;
                texts[3].SetActive(true);
                Collectable.Clear(Player.curStage);
                state = State.DONE;
                break;

            default: return;
        }
        
    }
}
