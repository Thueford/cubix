using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EnemySpawner : MonoBehaviour
{
    [NotNull] public Material[] modes;
    [NotNull] public EntityBase prefab;
    private MeshRenderer r;

    [Range(0, 3)] public int maxColors = 0;
    [Range(1, 100)] public int amount = 1;
    [Range(1,  10)] public int wavesize = 1;
    [Range(0, 100)] public float delay = 1;
    [Range(0, 10)] public float variation = 0.2f;

    [Tooltip("Probability for primary, secondary and tertiary color")]
    public Vector4 colors = new Vector4(1, 1, 1, 10);

    private int spawned = 0;
    private static int enemyCount = 0;

    private void Start()
    {
        r = GetComponent<MeshRenderer>();
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
        // if (Player.curStage == null)
        // { Invoke("Spawn", 0.1f); return; } // Tried to spawn without curStage

        if (enemyCount < Player.curStage.maxEnemies)
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
            EntityBase e = Instantiate(prefab, pos, Quaternion.identity, transform.parent);
            e.setColor(getWeightedColor());
            if (++spawned < amount) InitiateSpawn();
        }
        else InitiateSpawn();
    }

    void Update()
    {
        if (!Application.isPlaying && Time.frameCount % Application.targetFrameRate == 0)
        {
            if (prefab is E_Hunter) r.material = modes[2];
            else if (prefab is E_Archer) r.material = modes[3];
            else if (prefab is E_Stray) r.material = modes[4];
            else if (prefab is Player) r.material = modes[1];
            else r.material = modes[0]; 
            //Debug.Log(r.material);
        }
    }
    private Color getWeightedColor()
    {
        if (maxColors == 0) return Color.black;

        Vector4 prob = colors;                  //Probability vector
        prob /= Vector4.Dot(prob, Vector4.one); // normalize to sum=1

        Color c = Color.black;
        for (int i = 0; i < maxColors; i++)
        {
            float rand = Random.value;
            if (rand < prob.x) c += GameState.colorOrder[0];
            else if (rand < prob.x+prob.y) c += GameState.colorOrder[1];
            else if (rand < prob.x+prob.y+prob.z) c += GameState.colorOrder[2];
        }
        Debug.Log(c);
        return c;
    }
}
