using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Stage0 : StageController
{
    enum State { INIT, START, WASD, CHARGE, PORTAL }
    private State state;

    [Multiline] private string txtHighScore, txtStatistics;
    [NotNull] public Portal endPortal;

    private TMPro.TextMeshPro tmHIScore, tmStats;

    public void Awake()
    {
        tmHIScore = texts[3].GetComponent<TMPro.TextMeshPro>();
        tmStats = texts[5].GetComponent<TMPro.TextMeshPro>();
        txtHighScore = tmHIScore.text;
        txtStatistics = tmStats.text;
    }

    override public void ResetHints()
    {
        base.ResetHints();
        state = State.INIT;
    }

    private void updateStats()
    {
        tmHIScore.text = string.Format(txtHighScore, PlayerStats.self.getStartStats());
        tmStats.text = string.Format(txtStatistics, PlayerStats.self.getMoreStats());
    }

    override public void Experienced()
    {
        switch (state)
        {
            case State.INIT:
                endPortal.gameObject.SetActive(true);
                InvokeRepeating(nameof(updateStats), 0, .2f);
                texts[2].SetActive(true);
                texts[3].SetActive(true);
                texts[4].SetActive(true);
                texts[5].SetActive(true);
                break;
        }
    }

    override public void Newbie()
    {
        switch (state)
        {
            case State.INIT:
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
        switch (state)
        {
            case State.INIT:
                InputHandler.enableSpace = false;
                state++;
                break;
            case State.START:
                if (!Player.self.movable || InputHandler.ReadDirInput() == Vector3.zero) return;
                state++;
                break;
            case State.WASD:
                if (!GameState.curStage.charger.charged) return;
                state++;
                break;
            case State.CHARGE:
                state++;
                break;
            default: break;
        }
    }
}
