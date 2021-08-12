using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public static GUIManager self;

    private string heart = "\u2665";
    private int lastLives = 0;

    [NotNull] public TextMeshProUGUI tipText;
    [NotNull] public Text livesText;

    private void Awake()
    {
        if (!self) self = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        tipText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Player.self.lives != lastLives)
        {
            lastLives = Player.self.lives;
            string livesString = "";
            for (int i = 0; i < lastLives; i++)
                livesString += heart + " ";
            livesText.text = livesString;
        }
    }

    public void showTip(string text, float duration = 2)
    {
        tipText.SetText(text);
        tipText.gameObject.SetActive(true);
        StartCoroutine(E_FadeText(duration));
    }

    private IEnumerator E_FadeText(float duration)
    {
        float mult = duration;
        while (duration > 0)
        {
            if (!GameState.paused) 
            { 
                duration -= 0.05f;
                if (duration < mult / 2) 
                    tipText.alpha = Mathf.Sqrt((duration * 2) / mult);
            }
            yield return new WaitForSeconds(0.05f);
        }
        tipText.gameObject.SetActive(false);
    }
}
