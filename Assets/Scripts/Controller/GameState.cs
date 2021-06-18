using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameState : MonoBehaviour
{
    public static GameState self;
    [NotNull] public GameStage startStage;

    public static Color
        black = new Color(.3f, .3f, .3f, 1f),
        red = new Color(1f, 0f, 0f, 1f),
        green = new Color(0f, .7f, 0f, 1f),
        blue = new Color(0f, .3f, 1f, 1f),
        glow = new Color(.7f, .7f, .7f, 1f);

    public static Vector3Int unlockedColors = Vector3Int.zero;
    public static Color[] colorOrder = { Color.black, Color.black, Color.black };
    public static int maxActiveColors = 0, stage = 0;
    public static bool paused { get; private set; } = false;

    void Awake()
    {
       self = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (unlockedColors.x > 0) colorOrder[maxActiveColors++].r = 1;
        if (unlockedColors.y > 0) colorOrder[maxActiveColors++].g = 1;
        if (unlockedColors.z > 0) colorOrder[maxActiveColors++].b = 1;
        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0);
        // spawn in stage0
        if (startStage == null)
        {
            GameStage[] stages = FindObjectsOfType<GameStage>();
            Debug.Assert(stages.Length != 0, "no stages found");
            startStage = stages[0];
        }

        startStage.OnStageEntering();
        yield return new WaitForSeconds(0);
        Player.self.Teleport(startStage);
    }

    public static void TogglePause()
    {
        Debug.Log("TglPause");
        paused = !paused;
        if (paused)
        {
            Player.self.Freeze();
            Player.curStage.actors.SetActive(false);
        }
        else
        {
            Player.self.Melt();
            Player.curStage.actors.SetActive(true);
        }
    }

    public static void addRed()
    {
        if (unlockedColors.x != 1)
        {
            colorOrder[maxActiveColors].r = 1;
            unlockedColors.x = 1;
            maxActiveColors++;
        }
    }

    public static void addGreen()
    {
        if (unlockedColors.y != 1)
        {
            colorOrder[maxActiveColors].g = 1;
            unlockedColors.y = 1;
            maxActiveColors++;
        }
    }

    public static void addBlue()
    {
        if (unlockedColors.z != 1)
        {
            colorOrder[maxActiveColors].b = 1;
            unlockedColors.z = 1;
            maxActiveColors++;
        }
    }
}
