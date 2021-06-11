using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Vector3Int unlockedColors = Vector3Int.right;
    public int maxActiveColors = 0;

    void Awake()
    {
        self = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        maxActiveColors = unlockedColors.x + unlockedColors.y + unlockedColors.z;
        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.1f);
        // spawn in stage0
        if (startStage == null)
        {
            GameStage[] stages = FindObjectsOfType<GameStage>();
            Debug.Assert(stages.Length != 0, "no stages found");
            startStage = stages[0];
        }

        startStage.OnStageEntering();
        yield return new WaitForSeconds(0.1f);
        Player.self.Teleport(startStage);
    }

    public static void addRed()
    {
        if (self.unlockedColors.x != 1)
        {
            self.maxActiveColors += 1;
            self.unlockedColors.x = 1;
        }
    }

    public static void addGreen()
    {
        if (self.unlockedColors.y != 1)
        {
            self.maxActiveColors += 1;
            self.unlockedColors.y = 1;
        }
    }

    public static void addBlue()
    {
        if (self.unlockedColors.z != 1)
        {
            self.maxActiveColors += 1;
            self.unlockedColors.z = 1;
        }
    }
}
