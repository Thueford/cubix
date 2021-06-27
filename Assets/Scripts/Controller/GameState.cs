using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameState : MonoBehaviour
{
    public static GameState self;
    [NotNull] public GameStage startStage;
    [NotNull] public GameObject PauseOverlay;
    public static GameStage curStage;

    public static Color
        black = new Color(.3f, .3f, .3f, 1f),
        red = new Color(1f, 0f, 0f, 1f),
        green = new Color(0f, .7f, 0f, 1f),
        blue = new Color(0f, .3f, 1f, 1f),
        glow = new Color(.7f, .7f, .7f, 1f);

    public static Vector3Int unlockedColors = Vector3Int.zero;
    public static Color[] colorOrder = { Color.black, Color.black, Color.black };
    public static int colorCount = 0, stage = 0;
    public static bool paused { get; private set; } = false;

    [ReadOnly] public Color[] colorOrderNonStatic;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        if (unlockedColors.x > 0) colorOrder[colorCount++].r = 1;
        if (unlockedColors.y > 0) colorOrder[colorCount++].g = 1;
        if (unlockedColors.z > 0) colorOrder[colorCount++].b = 1;
        StartCoroutine(StartGame());
    }

    private void Update()
    {
        colorOrderNonStatic = colorOrder;
        if (InputHandler.ReadPauseInput()) TogglePause();
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

        startStage.Load();
        yield return new WaitForSeconds(0);
        Player.self.Teleport(startStage);
        stateBegin = SaveState();
    }

    public static void TogglePause()
    {
        Debug.Log("TglPause");
        paused = !paused;
        if (paused)
        {
            self.PauseOverlay.SetActive(true);

            Player.self.Freeze();
            curStage.actors.SetActive(false);
            curStage.charger.SetEnabled(false);
            foreach (EnemySpawner es in curStage.GetActorComponents<EnemySpawner>())
                es.StopSpawning();
        }
        else
        {
            Player.self.Melt();
            curStage.actors.SetActive(true);
            curStage.charger.SetEnabled(true);

            if (curStage.charger.charging)
            foreach (EnemySpawner es in curStage.GetActorComponents<EnemySpawner>())
                es.StartSpawning();

            self.PauseOverlay.SetActive(false);
        }
    }

    public static void load(State s)
    {
        Player.self.bs.updateProperties(Vector3Int.zero);
        Ressource.setModes(false);
        Ressource.self.addRes(Ressource.col.Red, s.resRed - Ressource.valueRed);
        Ressource.self.addRes(Ressource.col.Green, s.resGreen - Ressource.valueGreen);
        Ressource.self.addRes(Ressource.col.Blue, s.resBlue - Ressource.valueBlue);

        unlockedColors = s.unlockedColors;
        colorOrder = s.colorOrder;
        colorCount = s.colorCount;

        if (s.stage == curStage)
            s.stage.ResetStage();
            
        s.stage.Load();

        Player.self.Teleport(s.stage);
        Player.self.setHP(s.hp);
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
        s.colorOrder = colorOrder;
        s.unlockedColors = unlockedColors;
        return s;
    }

    public static void SaveCurState()
    {
        stateCurStage = SaveState();
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


}
