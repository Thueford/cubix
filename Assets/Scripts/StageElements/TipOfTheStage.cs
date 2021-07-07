using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipOfTheStage : MonoBehaviour
{
    [NotNull] public TMPro.TextMeshPro tots; //TipOfTheStageText
    [NotNull] public GameStage parentStage;

    // Start is called before the first frame update
    void Start()
    {
        //SetText
        tots.gameObject.SetActive(parentStage >= GameState.self.endlessStartStage);
    }


}
