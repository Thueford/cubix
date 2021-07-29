using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameState : MonoBehaviour
{
    public static GameState self;
    [NotNull] public GameStage startStage;
    [NotNull] public GameStage endlessStartStage;
    [NotNull] public GameObject PauseOverlay;
    [NotNull] public Portal endPortalPrefab;
    [NotNull] public Text txtDbg, txtFPS;
    public static GameStage curStage;
    public static PlayerStats playerStats;

    public static Color
        black = new Color(.3f, .3f, .3f, 1f),
        red = new Color(1f, 0f, 0f, 1f),
        green = new Color(0f, .7f, 0f, 1f),
        blue = new Color(0f, .3f, 1f, 1f),
        glow = new Color(.7f, .7f, .7f, 1f),
        gold = new Color(1, 0.84314f, 0, 1);

    public static Vector3Int unlockedColors = Vector3Int.zero;
    public static Color[] colorOrder = { Color.black, Color.black, Color.black };
    public static int colorCount = 0;
    public static bool paused { get; private set; } = false;

    [ReadOnly] public Color[] colorOrderNonStatic;
    [ReadOnly] public Vector3Int unlockedColorsNonStatic;

    public struct State
    {
        public GameStage stage;
        public Vector3Int unlockedColors;
        public Color[] colorOrder;
        public int colorCount;
        public float resRed, resGreen, resBlue, hp;
    }

    public static State stateCurStage;
    public static State stateBegin;
    public static State stateEndless;

    void Awake()
    {
        self = this;
        if (txtDbg == null) Debug.LogWarning("player.txtDbg not assigned");

        playerStats = PlayerStats.LoadProfile(1);
        playerStats.startNo++;
        playerStats.Save();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (unlockedColors.x > 0) colorOrder[colorCount++].r = 1;
        if (unlockedColors.y > 0) colorOrder[colorCount++].g = 1;
        if (unlockedColors.z > 0) colorOrder[colorCount++].b = 1;
        InvokeRepeating(nameof(UpdateFPS), 0, 0.5f);
        StartGame();
    }

    private void UpdateFPS() {
        txtFPS.text = (1 / Time.deltaTime).ToString("N0");
    }

    private void Update()
    {
        if (curStage == 0) playerStats.totalTime += Time.deltaTime;
        if (IsTutorial()) playerStats.tutorialTime += Time.deltaTime;
        colorOrderNonStatic = colorOrder;
        unlockedColorsNonStatic = unlockedColors;
        if (InputHandler.ReadPauseInput()) TogglePause();
    }

    public static void dbgSet(string msg) {
        if (self && self.txtDbg) self.txtDbg.text = msg;
    }
    public static void dbgLog(string msg) {
        if (self && self.txtDbg) self.txtDbg.text += "\n" + msg;
    }

    void StartGame()
    {
        // spawn in stage0
        if (startStage == null)
        {
            GameStage[] stages = FindObjectsOfType<GameStage>();
            Debug.Assert(stages.Length != 0, "no stages found");
            startStage = stages[0];
        }

        Player.self.SpawnAt(startStage);
        stateBegin = SaveState();
    }

    public static void TogglePause()
    {
        Debug.Log("TglPause");
        paused = !paused;
        if (paused)
        {
            Player.self.Freeze();
            curStage.actors.SetActive(false);
            curStage.charger.SetEnabled(false);
            self.PauseOverlay.SetActive(true);
        }
        else
        {
            self.PauseOverlay.SetActive(false);
            curStage.charger.SetEnabled(true);
            curStage.actors.SetActive(true);
            Player.self.Melt();
        }
    }

    public static void load(State s)
    {
        Player.self.SetShooterColor(Vector3Int.zero);
        Ressource.setModes(false);
        Ressource.self.addRes(Ressource.col.Red, s.resRed - Ressource.valueRed);
        Ressource.self.addRes(Ressource.col.Green, s.resGreen - Ressource.valueGreen);
        Ressource.self.addRes(Ressource.col.Blue, s.resBlue - Ressource.valueBlue);

        unlockedColors = s.unlockedColors;
        colorOrder = (Color[])s.colorOrder.Clone();
        colorCount = s.colorCount;

        Player.self.setHP(s.hp);
        Player.self.SpawnAt(s.stage);
    }

    public static void RestartStage()
    {
        load(stateCurStage);
    }

    public static void RestartGame()
    {
        load(stateBegin);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }

    public static State SaveState()
    {
        State s = new State();
        s.stage = curStage;
        s.hp = Player.self.HP;
        s.resRed = Ressource.valueRed;
        s.resGreen = Ressource.valueGreen;
        s.resBlue = Ressource.valueBlue;
        s.colorCount = colorCount;
        s.colorOrder = (Color[])colorOrder.Clone();
        s.unlockedColors = unlockedColors;
        return s;
    }

    public static void SaveCurState()
    {
        Debug.Log("SaveState");
        stateCurStage = SaveState();
    }

    public static void SwitchStage(GameStage next)
    {
        if (curStage != null)
        {
            if (curStage != next)
            {
                if (curStage.isProcedural)
                {
                    Destroy(curStage.gameObject);
                    Camera.main.transform.Translate(new Vector3(0, -40, 0));
                }
                else curStage.Unload();
            }
        }

        if (!playerStats.reachedEndless && next == 1)
            playerStats.stageDeaths = new int[11];

        next.Load();
        next.OnStageEnter();

        if (stateCurStage.stage != next)
            SaveCurState();
    }

    public static void updateClearStats(GameStage nextStage)
    {
        if (curStage + 1 > playerStats.stageHighscore)
            playerStats.stageHighscore = curStage + 1;

        if (!playerStats.reachedEndless && nextStage == self.endlessStartStage)
        {
            playerStats.colorOrder = colToStr(colorOrder[0]) + colToStr(colorOrder[1]) + colToStr(colorOrder[2]);
            playerStats.reachedEndless = true;
        }
        
        if (IsEndless()) playerStats.endlessClears++;
        if (IsTutorial()) playerStats.tutorialClears++;
        if (curStage) playerStats.totalClears++;

        playerStats.Save();
    }

    public static void updateDeadStats()
    {
        playerStats.totalDeaths++;

        if (!playerStats.reachedEndless && curStage < playerStats.stageDeaths.Length)
            playerStats.stageDeaths[curStage]++;

        if (!playerStats.reachedEndless)
            playerStats.tutorialDeaths++;

        playerStats.Save();
    }

    private static string colToStr(Color color)
    {
        if (color == Color.red) return "red";
        if (color == Color.green) return "green";
        if (color == Color.blue) return "blue";
        if (color == Color.black) return "black";
        if (color == Color.white) return "white";
        return "unknown";
    }

    public static Color getLightColor(Color color)
    {
        return color == black ? glow : color;
    }

    public static Color V2Color(Vector3Int rgb)
    {
        if (rgb == Vector3Int.zero) return black;
        else if (rgb == Vector3Int.forward) return blue;
        else if (rgb == Vector3Int.up) return green;
        else return new Color(rgb.x, rgb.y, rgb.z, 1f);
    }

    public static void addRed()
    {
        if (unlockedColors.x != 1)
        {
            colorOrder[colorCount].r = 1;
            unlockedColors.x = 1;
            colorCount++;
        }
    }

    public static void addGreen()
    {
        if (unlockedColors.y != 1)
        {
            colorOrder[colorCount].g = 1;
            unlockedColors.y = 1;
            colorCount++;
        }
    }

    public static void addBlue()
    {
        if (unlockedColors.z != 1)
        {
            colorOrder[colorCount].b = 1;
            unlockedColors.z = 1;
            colorCount++;
        }
    }

    public static bool IsEndless(GameStage s) => s >= self.endlessStartStage;
    public static bool IsEndless() => IsEndless(curStage);
    public static bool IsTutorial(GameStage s) => !IsEndless(s) && s > 0;
    public static bool IsTutorial() => IsTutorial(curStage);
}
