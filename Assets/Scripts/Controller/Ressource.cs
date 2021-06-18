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

    [Range(0, 100)]
    public static float valueRed = 50;
    [Range(0, 100)]
    public static float valueGreen = 50;
    [Range(0, 100)]
    public static float valueBlue = 50;

    public static float cooldown = 10;

    private static bool redMode = false;
    private static bool greenMode = false;
    private static bool blueMode = false;

    private const float alpha = 0.6862745098f;

    string bar = "█";

    public enum col
    {
        Red,
        Green,
        Blue
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!self) self = this;
        SetRessourceText(valueRed, TextRed);
        SetRessourceText(valueGreen, TextGreen);
        SetRessourceText(valueBlue, TextBlue);
        Space.SetActive(false);
        InvokeRepeating("CoolDown", 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        InputHandler.ReadSpaceInput();
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
        if (redMode)
        {
            addRes(col.Red, -cooldown);
            check(valueRed);
        }
        if (greenMode)
        {
            addRes(col.Green, -cooldown);
            check(valueGreen);
        }
        if (blueMode)
        {
            addRes(col.Blue, -cooldown);
            check(valueBlue);
        }
    }

    private void check(float value)
    {
        if (value == 0)
        {
            InputHandler.enableNumbers = true;
            Player.self.bs.updateProperties(Vector3Int.zero);
            CancelInvoke("CoolDown");
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


    public void addRes(col c, float value)
    {
        switch (c)
        {
            case col.Red:
                valueRed = Mathf.Max(0, Mathf.Min(100, valueRed + value));
                SetRessourceText(valueRed, TextRed);
                break;
            case col.Green:
                valueGreen = Mathf.Max(0, Mathf.Min(100, valueGreen + value));
                SetRessourceText(valueGreen, TextGreen);
                break;
            case col.Blue:
                valueBlue = Mathf.Max(0, Mathf.Min(100, valueBlue + value));
                SetRessourceText(valueBlue, TextBlue);
                break;
            default:
                break;
        }
    }

}
