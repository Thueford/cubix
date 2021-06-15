using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EnemySpawner : MonoBehaviour
{
    [NotNull] public Material[] modes;
    [NotNull] public EntityBase prefab;
    private MeshRenderer r;

    [Range(1, 100)]
    public int amount = 1;
    public float delay = 1;
    public float variation = 0.2f;

    private int spawned = 0;
    private static int enemyCount = 0;

    private void Start()
    {
        r = GetComponent<MeshRenderer>();
        r.enabled = !Application.isPlaying;
        if(Application.isPlaying) Invoke("Spawn", Random.value * variation);
    }

    public void Reset()
    {
        Debug.LogWarning("Spawner Reset");
    }

    public void ResetSpawner()
    {
        spawned = 0;
    }

    public static void EnemyDied()
    {
        enemyCount--;
    }

    public static void EnemySpawned()
    {
        enemyCount++;
    }

    public void InitiateSpawn()
    {
        Invoke("Spawn", delay + Random.value * variation);
    }

    private void Spawn()
    {
        if(enemyCount < Player.curStage.maxEnemies)
        {
            Vector3 pos = transform.position + new Vector3((Random.value - 0.5f) * transform.lossyScale.x, 0, (Random.value - 0.5f) * transform.lossyScale.z);
            pos.y = 0.5f;
            EntityBase e = Instantiate(prefab, pos, Quaternion.identity, transform.parent);
            if (++spawned < amount) InitiateSpawn();
        }
        else InitiateSpawn();
    }

    void Update()
    {
        if(Time.frameCount % Application.targetFrameRate == 0)
        {
            if (prefab is E_Hunter) r.material = modes[2];
            else if (prefab is E_Archer) r.material = modes[3];
            else if (prefab is E_Stray) r.material = modes[4];
            else if (prefab is Player) r.material = modes[1];
            else r.material = modes[0];
        }
    }
}
