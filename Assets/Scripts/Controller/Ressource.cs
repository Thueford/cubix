using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ressource : MonoBehaviour
{
    public static Ressource self;

    [NotNull] public Text TextRed;
    [NotNull] public Text TextGreen;
    [NotNull] public Text TextBlue;
    [NotNull] public GameObject Space;
    [NotNull] public ResParts resPartsPrefab;

    [Range(0, 100)]
    public float valueRed = 0;
    [Range(0, 100)]
    public float valueGreen = 0;
    [Range(0, 100)]
    public float valueBlue = 0;

    public float cooldown = 10;

    private bool redMode = false;
    private bool greenMode = false;
    private bool blueMode = false;

    private const float alpha = 0.6862745098f;

    const string bar = "█";

    public enum col { Red, Green, Blue}

    // Start is called before the first frame update
    void Start()
    {
        if (!self) self = this;
        SetRessourceText(valueRed, TextRed);
        SetRessourceText(valueGreen, TextGreen);
        SetRessourceText(valueBlue, TextBlue);
        Space.SetActive(false);
        InvokeRepeating(nameof(CoolDown), 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (InputHandler.enableSpace)
        {
            bool red = valueRed >= 100;
            bool green = valueGreen >= 100;
            bool blue = valueBlue >= 100;

            int i = (red ? 1 : 0) +
                (green ? 1 : 0) +
                (blue ? 1 : 0);

            if (i >= 2) Space.SetActive(true);
            else Space.SetActive(false);
        }
    }

    void CoolDown()
    {
        if (GameState.paused) return;
        if (redMode) check(addRes(col.Red, -cooldown));
        if (greenMode) check(addRes(col.Green, -cooldown));
        if (blueMode) check(addRes(col.Blue, -cooldown));
    }

    private void check(float value)
    {
        if (value == 0) setModes(false);
    }

    public void setModes(bool b)
    {
        redMode = greenMode = blueMode = b;
        if (!b)
        {
            InputHandler.enableNumbers = true;
            if ((GameState.unlockedColors * Player.self.bs.lastColor).sqrMagnitude > 0)
                Player.self.SetShooterColor(Player.self.bs.lastColor);
            SoundHandler.SetHPTarget(10);
        }
    }

    void SetRessourceText(float value, Text textObject)
    {
        Color color = textObject.color;
        string tmp = "";

        if (value == 100)
        {
            color.a = 1;
            textObject.color = color;
        }
        else
        {
            color.a = alpha;
            textObject.color = color;
        }

        for (int i = 0; i < (int)value / 10; i++)
        {
            tmp += bar + " ";
        }
        textObject.text = tmp;
    }

    public Vector3Int activateColors()
    {
        Vector3Int rgb = Vector3Int.zero;
        bool red = valueRed >= 100;
        bool green = valueGreen >= 100;
        bool blue = valueBlue >= 100;

        int i = (red ? 1 : 0) +
            (green ? 1 : 0) +
            (blue ? 1 : 0);

        if (i < 2) return rgb;

        InputHandler.enableNumbers = false;

        redMode = red;
        rgb.x = red ? 1 : 0;

        greenMode = green;
        rgb.y = green ? 1 : 0;

        blueMode = blue;
        rgb.z = blue ? 1 : 0;

        SoundHandler.SetHPTarget(500);

        return rgb;
    }


    public float addRes(col c, float value)
    {
        switch (c)
        {
            case col.Red:
                if (valueRed < 100 && !redMode && valueRed + value >= 100)
                    SoundHandler.PlayClip("resFull");
                valueRed = Mathf.Clamp(valueRed + value, 0, 100);
                SetRessourceText(valueRed, TextRed);
                return valueRed;
            case col.Green:
                if (valueGreen < 100 && !greenMode && valueGreen + value >= 100)
                    SoundHandler.PlayClip("resFull");
                valueGreen = Mathf.Clamp(valueGreen + value, 0, 100);
                SetRessourceText(valueGreen, TextGreen);
                return valueGreen;
            case col.Blue:
                if (valueBlue < 100 && !blueMode && valueBlue + value >= 100)
                    SoundHandler.PlayClip("resFull");
                valueBlue = Mathf.Clamp(valueBlue + value, 0, 100);
                SetRessourceText(valueBlue, TextBlue);
                return valueBlue;
            default:
                return 0;
        }
    }
}
