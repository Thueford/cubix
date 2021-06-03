using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene2 : Hint
{
    enum State { START,  SHOOT }
    private State state;

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.START:
                if (Player.curStage != GetComponent<GameStage>()) return;
                texts[0].SetActive(true);
                break;

            default: return;
        }
        state++;
    }
}
