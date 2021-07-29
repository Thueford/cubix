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
    [Range(0,   3)] public int maxColors = 3;
    [Range(1, 100)] public int amount = 10;
    [Range(1,  10)] public int wavesize = 2;
    [Range(0, 100)] public float initDelay = 0;
    [Range(0, 100)] public float delay = 5;
    [Range(0,  10)] public float variation = 1f;

    private static float enemyCount = 0;
    private static int maxSpawning = 0;

    private int spawned = 0;
    private MeshRenderer r;
    private float initTimer;
    private bool isSpawning = false;

    void Awake()
    {
        initTimer = initDelay;
        r = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        r.enabled = !Application.isPlaying;
    }

    private void Update()
    {
        if (isSpawning && initTimer != 0)
        {
            initTimer -= Time.deltaTime;
            if (initTimer <= 0)
            {
                initTimer = 0;
                SetSpawning(true);
            }
        }
    }

    public static void EnableSpawning(GameStage stage, bool b)
    {
        foreach (EnemySpawner es in stage.GetActorComponents<EnemySpawner>())
            es.SetSpawning(b);
    }

    public void SetSpawning(bool b)
    {
        isSpawning = b;
        if (initTimer <= 0)
        {
            if (b) InvokeRepeating(nameof(SpawnWave), 0, delay);
            else CancelInvoke();
        }
    }


    private static float round(float f, float d = 1e3f) => Mathf.RoundToInt(f * d) / d;

    public static void Reset(GameStage stage)
    {
        enemyCount = 0;
        maxSpawning = 0;
        foreach (EnemySpawner es in stage.GetActorComponents<EnemySpawner>())
            es.ResetSpawner();
    }

    public void ResetSpawner()
    {
        spawned = 0;
        maxSpawning += amount;
        initTimer = initDelay;
    }

    public static void EnemyDied(EnemyBase e)
    {
        enemyCount = round(enemyCount - e.countWeight);
        if (--maxSpawning == 0)
        {
            GameState.curStage.charger.EnableParticles(true);
            GameState.curStage.charger.SetChargeSpeed(1.5f);
        }
    }

    public static void EnemySpawned(EnemyBase e)
    {
        ++maxSpawning;
        enemyCount = round(enemyCount + e.countWeight);
    }

    // spawns a wave of enemies with variation
    private void SpawnWave()
    {
        for (int i = 0; i < wavesize; i++)
            Invoke(nameof(Spawn), Random.value * variation);
    }

    // spawns an enemy with effects
    private void Spawn()
    {
        if (spawned >= amount || enemyCount >= GameState.curStage.maxEnemies) return;

        Color col = getWeightedColor();
        int n = EnemyBase.getColorCount(col);

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

        --maxSpawning;
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
