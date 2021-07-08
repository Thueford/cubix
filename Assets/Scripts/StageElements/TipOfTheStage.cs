using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipOfTheStage : MonoBehaviour
{
    [NotNull] public TMPro.TextMeshPro tots; //TipOfTheStageText
    [NotNull] public GameStage parentStage;

    static string[] tips = { };
    static int lastTip = -1;

    private void Awake()
    {
        if (tips.Length == 0) tips = Resources.Load<TextAsset>("StageTips").text.Trim().Split('\n');
    }

    // Start is called before the first frame update
    void Start()
    {
        int tip, maxTries = 10;
        do tip = Random.Range(0, tips.Length);
        while (tip == lastTip && maxTries-- > 0);

        tots.gameObject.SetActive(parentStage >= GameState.self.endlessStartStage);
        tots.text = tips[tip].Trim();
    }
}
