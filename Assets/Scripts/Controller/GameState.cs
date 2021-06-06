using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState self;
    public GameObject startStage;

    public static Color
        black = new Color(.3f, .3f, .3f, 1f),
        blue = new Color(0f, .3f, 1f, 1f),
        green = new Color(0f, .7f, 0f, 1f),
        glow = new Color(.7f, .7f, .7f, 1f);

    public static Vector3Int unlockedColors = Vector3Int.zero;
    public static int maxActiveColors = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (self == null) self = this;
        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.2f);

        // spawn in stage0
        if (startStage == null)
        {
            GameStage[] stages = FindObjectsOfType<GameStage>();
            Debug.Assert(stages.Length != 0, "no stages found");
            Player.self.Teleport(stages[0]);
        }
        else Player.self.Teleport(startStage.GetComponent<GameStage>());
    }
}
