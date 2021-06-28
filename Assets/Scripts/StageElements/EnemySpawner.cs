using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [NotNull, HideInInspector] 
    public EntityBase b_archer, b_hunter, b_stride;
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
    [Range(0, 100)] public float delay = 5;
    [Range(0,  10)] public float variation = 1f;

    private int spawned = 0;
    private MeshRenderer r;
    private static int enemyCount = 0;

    void Awake()
    {
        r = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        r.enabled = !Application.isPlaying;
    }

    public void StartSpawning() { SpawnWave(); }
    public void StopSpawning() { CancelInvoke(); }
    public void ResetSpawner() { spawned = 0; }
    public static void EnemyDied() { enemyCount--; }
    public static void EnemySpawned() { enemyCount++; }

    public void InitiateSpawn()
    {
        Invoke("SpawnWave", delay + Random.Range(-variation, variation));
    }

    private void SpawnWave()
    {
        Invoke("Spawn", Random.value * variation);
    }

    private void Spawn()
    {
        // if (GameState.curStage == null)
        // { Invoke("Spawn", 0.1f); return; } // Tried to spawn without curStage

        if (enemyCount < GameState.curStage.maxEnemies)
        {
            Vector3 pos;
            int tries = 10;
            do pos = transform.position + new Vector3((Random.value - 0.5f) * transform.lossyScale.x, 0, (Random.value - 0.5f) * transform.lossyScale.z);
            while (Vector3.Distance(pos, Player.self.transform.position) < 2 && --tries > 0);
            
            if (tries == 0) {
                InitiateSpawn();
                Debug.LogWarning("max spawn tries reached");
                return;
            }

            pos.y = 0.5f;
            EntityBase e = Instantiate(getRandomPrefab(), pos, Quaternion.identity, transform.parent);
            e.setColor(getWeightedColor());
            if (++spawned < amount) InitiateSpawn();
        }
        else InitiateSpawn();
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

        Debug.Log(l.ToArray());
        for (int i = 0; i < maxColors && l.Count > 0; i++)
        {
            int x = Random.Range(0, l.Count);
            c += GameState.colorOrder[l[x]];
            Debug.Log("WC: " + x + " " + l[x] + " -> " + c);
            l.RemoveAt(x);
        }

        return c;
    }

    private EntityBase getRandomPrefab()
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
