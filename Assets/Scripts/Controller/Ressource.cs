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

    [Range(0, 100)]
    public static float valueRed = 100;
    [Range(0, 100)]
    public static float valueGreen = 100;
    [Range(0, 100)]
    public static float valueBlue = 100;

    public static float cooldown = 10;

    private static bool redMode = false;
    private static bool greenMode = false;
    private static bool blueMode = false;

    string bar = "█";

    // Start is called before the first frame update
    void Start()
    {
        if (!self) self = this;
        SetRessourceText(valueRed, TextRed);
        SetRessourceText(valueGreen, TextGreen);
        SetRessourceText(valueBlue, TextBlue);
        InvokeRepeating("CoolDown", 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        InputHandler.ReadSpaceInput();
    }

    void CoolDown()
    {
        if (redMode)
        {
            valueRed = safeCoolDown(valueRed);
            SetRessourceText(valueRed, TextRed);
        }
        if (greenMode)
        {
            valueGreen = safeCoolDown(valueGreen);
            SetRessourceText(valueGreen, TextGreen);
        }
        if (blueMode)
        {
            valueBlue = safeCoolDown(valueBlue);
            SetRessourceText(valueBlue, TextBlue);
        }
    }

    private float safeCoolDown(float value)
    {
        value = Mathf.Max(0, value - cooldown);
        if (value == 0)
        {
            InputHandler.enableNumbers = true;
            Player.self.bs.updateProperties(Vector3Int.zero);
        }
        return value;
    }

    void SetRessourceText(float value, Text textObject)
    {
        string tmp = "";
        for (int i = 0; i < (int)value / 10; i++)
        {
            tmp += bar + " ";
        }
        textObject.text = tmp;
    }

    public static void activateColors()
    {
        bool red = valueRed >= 100;
        bool green = valueGreen >= 100;
        bool blue = valueBlue >= 100;

        int i = (red ? 1 : 0) +
            (green ? 1 : 0) +
            (blue ? 1 : 0);

        if (i < 2) return;

        InputHandler.enableNumbers = false;

        Vector3Int rgb = Vector3Int.zero;

        redMode = red;
        rgb.x = red ? 1 : 0;

        greenMode = green;
        rgb.y = green ? 1 : 0;

        blueMode = blue;
        rgb.z = blue ? 1 : 0;

        Player.self.bs.updateProperties(rgb);
    }

    public void addRed(float value)
    {
        valueRed += value;
        SetRessourceText(valueRed, TextRed);
    }

    public void addGreen(float value)
    {
        valueGreen += value;
        SetRessourceText(valueGreen, TextGreen);
    }
    public void addBlue(float value)
    {
        valueBlue += value;
        SetRessourceText(valueBlue, TextBlue);
    }
}
