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

    override public void Experienced()
    {
        switch (state)
        {
            case State.START:
                endPortal.gameObject.SetActive(true);
                texts[3].GetComponent<TMPro.TextMeshPro>().text =
                    string.Format(txtHighScore, GameState.settings.stageHighscore);
                texts[3].SetActive(true);
                texts[2].SetActive(true);
                texts[4].SetActive(true);
                break;
        }
    }

    override public void Newbie()
    {
        switch (state)
        {
            case State.START:
                endPortal.gameObject.SetActive(false);
                texts[0].SetActive(true);
                break;
            case State.WASD:
                texts[1].SetActive(true);
                break;
            case State.CHARGE:
                texts[2].SetActive(true);
                break;
        }
    }

    public override void General()
    {
        base.General();
        switch (state)
        {
            case State.START:
                InputHandler.enableSpace = false;
                if (InputHandler.ReadDirInput() == Vector3.zero) return;
                state++;
                break;
            case State.WASD:
                if (!GameState.curStage.portal.Enabled()) return;
                state++;
                break;
            case State.CHARGE:
                state++;
                break;
            default: break;
        }
    }
}
