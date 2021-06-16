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
    [Tooltip("Probability for primary, secondary and tertiary color")]
    public Vector4 colors = new Vector4(1, 1, 1, 10);

    private int spawned = 0;
    private static int enemyCount = 0;

    private void Start()
    {
        r = GetComponent<MeshRenderer>();
        r.enabled = !Application.isPlaying;
        if (Application.isPlaying) Invoke("Spawn", variation);
    }

    private Color getWeightedColor()
    {
        Vector4 color = (Vector3)GameState.self.unlockedColors; // active colors
        color.w = GameState.self.maxActiveColors;      // default color scale
        color.Scale(colors);                      // apply scales
        color /= Vector4.Dot(color, Vector4.one); // normalize to sum=1

        Color c = Color.black;
        if (Random.value < color.x) c += GameState.colorOrder[0];
        if (Random.value < color.y) c += GameState.colorOrder[1];
        if (Random.value < color.z) c += GameState.colorOrder[2];

        return c;
    }

    public void ResetSpawner() { spawned = 0; }
    public static void EnemyDied() { enemyCount--; }
    public static void EnemySpawned() { enemyCount++; }

    public void InitiateSpawn()
    {
        Invoke("Spawn", delay + Random.value * variation);
    }

    private void Spawn()
    {
        // if (Player.curStage == null)
        // { Invoke("Spawn", 0.1f); return; } // Tried to spawn without curStage

        if (enemyCount < Player.curStage.maxEnemies)
        {
            Vector3 pos = transform.position + new Vector3((Random.value - 0.5f) * transform.lossyScale.x, 0, (Random.value - 0.5f) * transform.lossyScale.z);
            pos.y = 0.5f;
            EntityBase e = Instantiate(prefab, pos, Quaternion.identity, transform.parent);
            e.setColor(getWeightedColor());
            if (++spawned < amount) InitiateSpawn();
        }
        else InitiateSpawn();
    }

    void Update()
    {
        if (Time.frameCount % Application.targetFrameRate == 0)
        {
            if (prefab is E_Hunter) r.material = modes[2];
            else if (prefab is E_Archer) r.material = modes[3];
            else if (prefab is E_Stray) r.material = modes[4];
            else if (prefab is Player) r.material = modes[1];
            else r.material = modes[0];
        }
    }
}
