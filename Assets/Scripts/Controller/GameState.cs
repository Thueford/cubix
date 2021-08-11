using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Helper;

[DisallowMultipleComponent]
public class GameState : MonoBehaviour
{
    public static GameState self;
    public static GameStage curStage;
    public static SaveData.Data save;
    // public SaveData.Data _save;

    public static State stateCurStage;
    public static State stateBegin;
    public static State stateEndless;

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
    public static bool showMenu = false;

    [NotNull] public GameStage startStage;
    [NotNull] public GameStage endlessStartStage;
    [NotNull] public GameObject PauseOverlay;
    [WarnNull] public Transform effectContainer;
    [NotNull] public Text txtDbg, txtFPS;
    public Texture2D menuBackground;

    [ReadOnly] public Color[] colorOrderNonStatic;
    [ReadOnly] public Vector3Int unlockedColorsNonStatic;

    private float lastFpsTime = 0;


    public static int endlessNo => self && self.endlessStartStage ? self.endlessStartStage : 1000;

    // difficulty slope for factor f at stage s: (f - 1) / s
    private const float HPslope = (2 - 1) / 20;
    public static float HPfactor => curStage > endlessNo ? 1 + HPslope * (curStage - 10) : 1;


    void Awake()
    {
        self = this;
        if (effectContainer == null) effectContainer = gameObject.transform;
        if (txtDbg == null) Debug.LogWarning("player.txtDbg not assigned");
        else dbgSet("");

        save = new SaveData.Data().Load(1);
        save.stats.startNo++;
        save.stats.Save();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (unlockedColors.x > 0) colorOrder[colorCount++].r = 1;
        if (unlockedColors.y > 0) colorOrder[colorCount++].g = 1;
        if (unlockedColors.z > 0) colorOrder[colorCount++].b = 1;
        save.config.UpdateSettings();
        //_save = save;
        save.config.setLights();
        Invoke(nameof(StartGame), 0);
    }


    private void UpdateFPS()
    {
        lastFpsTime = Time.time;
        int fps = (int)(1 / Time.deltaTime);
        if (txtFPS) txtFPS.text = fps.ToString("N0");
        save.stats.fpsSum += fps;
        save.stats.fpsCount++;
        if (fps < save.stats.fpsMin && fps > 0) save.stats.fpsMin = fps;
        if (fps > save.stats.fpsMax) save.stats.fpsMax = fps;
    }

    private void Update()
    {
        if (Time.time > lastFpsTime + 0.5) UpdateFPS();
        if (!paused) StageStats.cur.Charge(curStage.charger);
        if (InputHandler.ReadPauseInput()) TogglePause();
        save.config.ReadConfigShortcuts();

        if (IsTutorial()) save.stats.tutorialTime += Time.deltaTime;
        save.stats.totalTime += Time.deltaTime;

        colorOrderNonStatic = colorOrder;
        unlockedColorsNonStatic = unlockedColors;
    }

    private void OnGUI()
    {
        if (showMenu)
        {
            if (!paused) TogglePause();
            save.config.ConfigMenu();
        }
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
        Player.self.SetShooterColor(Vector3Int.zero);
        stateBegin = SaveState();
    }

    public static void TogglePause()
    {
        if (paused = !paused)
        {
            Debug.Log("Pause");
            Player.self.Freeze();
            curStage.actors.SetActive(false);
            curStage.charger.SetEnabled(false);
            self.PauseOverlay.SetActive(true);
        }
        else
        {
            Debug.Log("Unpause");
            showMenu = false;
            self.PauseOverlay.SetActive(false);
            curStage.charger.SetEnabled(true);
            curStage.actors.SetActive(true);
            Player.self.Melt();
        }
    }

    public static void ToggleSettings()
    {
        showMenu = !showMenu;
        if (showMenu && !paused) TogglePause();
    }

    public static void load(State s)
    {
        Player.self.SetShooterColor(Vector3Int.zero);
        
        unlockedColors = s.unlockedColors;
        colorOrder = System.Array.ConvertAll(s.colorOrder, c => B2C(c));
        colorCount = s.colorCount;

        Ressource.self.setModes(false);
        Ressource.self.addRes(Ressource.col.Red, s.ressources[0] - Ressource.self.valueRed);
        Ressource.self.addRes(Ressource.col.Green, s.ressources[1] - Ressource.self.valueGreen);
        Ressource.self.addRes(Ressource.col.Blue, s.ressources[2] - Ressource.self.valueBlue);

        Player.self.setHP(s.hp);
        Player.self.SpawnAt(s.stage);
        save.Save();
    }

    public static void RestartStage()
    {
        save.stats.stageRestarts++;
        load(stateCurStage);
    }

    public static void RestartGame()
    {
        load(stateBegin);
    }

    public static void QuitGame()
    {
        save.Save();
        Application.Quit();
    }

    public static State SaveState()
    {
        State s = new State();
        s.stage = curStage;
        s.hp = Player.self.HP;
        s.ressources = new float[] { 
            round(Ressource.self.valueRed),
            round(Ressource.self.valueGreen),
            round(Ressource.self.valueBlue)};
        s.colorCount = colorCount;
        s.colorOrder = System.Array.ConvertAll(colorOrder, c => C2B(c));
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
        if (curStage != null && curStage != next) 
            curStage.Unload();

        if (!save.stats.reachedEndless && next == 1)
            save.stats.stageDeaths = new int[11];

        curStage = next;
        next.Load();
        next.OnStageEnter();

        if (stateCurStage.stage != next) SaveCurState();

        save.stags.addStage(next, stateCurStage);
    }

    public static void updateClearStats(GameStage nextStage)
    {
        if (curStage + 1 > save.stats.stageHighscore)
            save.stats.stageHighscore = curStage + 1;

        if (!save.stats.reachedEndless && nextStage == endlessNo)
        {
            save.stats.colorOrder = colToStr(colorOrder[0]) + colToStr(colorOrder[1]) + colToStr(colorOrder[2]);
            save.stats.reachedEndless = true;
        }
        
        if (IsEndless()) save.stats.endlessClears++;
        if (IsTutorial()) save.stats.tutorialClears++;
        if (curStage) save.stats.totalClears++;

        save.SaveStats();
    }

    public static void updateDeadStats()
    {
        save.stats.totalDeaths++;

        if (!save.stats.reachedEndless && curStage < save.stats.stageDeaths.Length)
            save.stats.stageDeaths[curStage]++;

        if (!save.stats.reachedEndless)
            save.stats.tutorialDeaths++;

        save.SaveStats();
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

    public static bool IsEndless(GameStage s) => s >= endlessNo;
    public static bool IsEndless() => IsEndless(curStage);
    public static bool IsTutorial(GameStage s) => !IsEndless(s) && s > 0;
    public static bool IsTutorial() => IsTutorial(curStage);

    [System.Serializable]
    public struct State
    {
        public GameStage stage;
        public Vector3Int unlockedColors;
        public int[] colorOrder;
        public int colorCount;
        public float hp;
        public float[] ressources;
    }
}
