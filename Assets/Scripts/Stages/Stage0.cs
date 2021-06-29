using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage0 : StageController
{
    enum State { START, WASD, CHARGE, PORTAL }
    private State state;

    [Multiline] public string txtHighScore = "HighScore:\nStage: {0}";
    [NotNull] public Portal endPortal;

    override public void ResetHints()
    {
        base.ResetHints();
        state = State.START;
    }

    override public void EndlessHints()
    {
        switch (state)
        {
            case State.START:
                endPortal.gameObject.SetActive(true);
                texts[3].GetComponent<TMPro.TextMeshPro>().text =
                    string.Format(txtHighScore, GameState.settings.stageHighscore);
                texts[3].SetActive(true);
                break;
        }
        state++;
    }

    override public void NewbieHints()
    {
        switch (state)
        {
            case State.START:
                endPortal.gameObject.SetActive(false);
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
                if (!GameState.curStage.portal.Enabled()) return;
                texts[2].SetActive(true);
                break;

            default: return;
        }
        state++;
    }
}
