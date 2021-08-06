using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [NotNull, HideInInspector] 
    public EnemyBase b_archer, b_hunter, b_stride;
    [Header("Type Probes")]
    [Range(0, 100)] public int p_hunter = 100;
    [Range(0, 100)] public int p_archer = 100;
    [Range(0, 100)] public int p_stride = 100;

    [Header("Color Probes")]
    [Range(0, 100)] public int p_prim = 0;
    [Range(0, 100)] public int p_secd = 0;
    [Range(0, 100)] public int p_tert = 0;

    [Header("Other")]
    [Range(0, 3)] public int maxColors = 3;
    [Range(1, 100)] public int amount = 10;
    [Range(1, 10)] public int wavesize = 2;
    [Range(0, 100)] public float initDelay = 0;
    [Range(0, 100)] public float delay = 5;
    [Range(0, 10)] public float variation = 1f;

    public static float enemyWeight { get; private set; } = 0; // current enemy weight
    public static int enemyCount { get; private set; } = 0; // current enemy count
    public static int remaining { get; private set; } = 0; // enemies left to kill

    private int spawned = 0;
    private MeshRenderer r;
    private float spawnTimer;
    private bool isSpawning = false;

    void Awake()
    {
        spawnTimer = initDelay;
        r = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        r.enabled = !Application.isPlaying;
    }

    private void Update()
    {
        if (isSpawning)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnWave();
                spawnTimer = delay;
            }
        }
    }

    public static void EnableSpawning(GameStage stage, bool b)
    {
        Debug.Log("Enable " + stage.isBoss + " " + b + " " + stage.actors.GetComponentsInChildren<EnemyBase>(true).Length);
        if (stage.isBoss && b)
            foreach (EnemyBase e in stage.actors.GetComponentsInChildren<EnemyBase>(true))
                e.gameObject.SetActive(true);
        
        foreach (EnemySpawner es in stage.GetActorComponents<EnemySpawner>())
            es.SetSpawning(b);
    }

    public void SetSpawning(bool b) => isSpawning = b;


    private static float round(float f, float d = 1e3f) => Mathf.RoundToInt(f * d) / d;

    public static void Reset(GameStage stage)
    {
        enemyWeight = 0;
        enemyCount = 0;
        remaining = stage.isBoss ? 1 : 0;
        foreach (EnemySpawner es in stage.GetActorComponents<EnemySpawner>())
            es.ResetSpawner();
        Debug.Log("Remaining: " + remaining);
    }

    public void ResetSpawner()
    {
        spawned = 0;
        remaining += amount;
        spawnTimer = initDelay;
    }

    public static void EnemyDied(EnemyBase e)
    {
        enemyWeight = round(enemyWeight - e.countWeight);
        --enemyCount;
        if (--remaining == 0) GameState.curStage.FinishedEnemies();
    }

    public static void EnemySpawned(EnemyBase e)
    {
        ++enemyCount;
        enemyWeight = round(enemyWeight + e.countWeight);
    }

    // spawns a wave of enemies with variation
    private void SpawnWave()
    {
        if (spawned < amount && enemyCount < GameState.curStage.maxEnemies)
            for (int i = 0; i < wavesize; i++)
                Invoke(nameof(Spawn), Random.value * variation);
    }

    // spawns an enemy with effects
    private void Spawn()
    {
        Color col = getWeightedColor();
        int n = EnemyBase.getColorCount(col);
        if (n > 1) remaining += n - 1;

        while (n-- > 0)
        {
            Vector3 pos;
            int tries = 10;
            do pos = transform.position + new Vector3(
                (Random.value - 0.5f) * transform.lossyScale.x, 0.5f,
                (Random.value - 0.5f) * transform.lossyScale.y);
            while (Vector3.Distance(pos, Player.self.pos) < 8 && --tries > 0);
            if (tries == 0) break;

            EnemyBase e = Instantiate(getRandomPrefab(), pos, Quaternion.identity, transform.parent);
            e.setColor(col);
        }

        ++spawned;
    }

    // This is madness ngl
    private Color getWeightedColor()
    {
        if (maxColors == 0) return Color.black;
        List<int> l = new List<int>();

        Color c = Color.black;
        if (100 * Random.value < p_prim) l.Add(0);
        if (100 * Random.value < p_secd) l.Add(1);
        if (100 * Random.value < p_tert) l.Add(2);

        for (int i = 0; i < maxColors && l.Count > 0; i++)
        {
            int x = Random.Range(0, l.Count);
            c += GameState.colorOrder[l[x]];
            //Debug.Log("WC: " + x + " " + l[x] + " -> " + c);
            l.RemoveAt(x);
        }

        return c;
    }

    private EnemyBase getRandomPrefab()
    {
        int sum = p_hunter + p_archer + p_stride;
        int val = Mathf.RoundToInt(sum * Random.value);

        if ((val -= p_hunter) <= 0) return b_hunter;
        if ((val -= p_archer) <= 0) return b_archer;
        if ((val -= p_stride) <= 0) return b_stride;
        Debug.LogError("Why does this happen to me");
        return null;
    }
}
