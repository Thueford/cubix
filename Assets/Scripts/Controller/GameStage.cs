using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameStage : MonoBehaviour
{
    [NotNull, HideInInspector] public Camera cam;
    [NotNull, HideInInspector] public Spawn spawn;
    [NotNull, HideInInspector] public Charger charger;
    [NotNull, HideInInspector] public Portal portal;
    [NotNull, HideInInspector] public GameObject actorsBase;
    [NotNull, HideInInspector] public TMPro.TextMeshPro stageText;
    public GameObject actors;
    [WarnNull] public GameStage next;
    [WarnNull] public StageController hints;

    [Range(1,100)] public float chargeTime = 10;
    [Range(1,100)] public int maxEnemies = 10;

    public int number;
    public bool loaded { get; private set; } = false;
    public bool isProcedural = false;

    public static implicit operator int(GameStage s) => s.number;

    public void Awake()
    {
        gameObject.SetActive(false);
        actorsBase.SetActive(false);
    }

    public void Start()
    {
        stageText.text = "Stage: " + number;
    }

    // Reset entities
    public void ResetStage()
    {
        /*
        foreach (EnemySpawner eb in GetActorComponents<EnemySpawner>())
            eb.ResetSpawner();
        */
        if (actors) Destroy(actors);
        actors = Instantiate(actorsBase, transform.position, Quaternion.identity, transform);
        portal.SetEnabled(false);
        if (hints) hints.ResetHints();

        charger.Reset(chargeTime);
        EnemySpawner.ResetEnemyCount();
    }

    // Called when player steps on portal
    public void Load()
    {
        if (loaded) return;
        GameState.curStage = this;
        gameObject.SetActive(true);
        ResetStage();
        loaded = true;
    }

    // Called when player is spawning
    public void OnStageEnter()
    {
        Debug.Log("Stage: " + name);

        // copy camera
        Camera.main.GetComponent<GameCamera>().target = cam.transform.position;
        Camera.main.transform.rotation = cam.transform.rotation;

        actors.SetActive(true);
        spawn.Disable();

        //if(next != null) next.spawn.Enable();
    }

    // Called after player finished spawning
    public void MeltActors()
    {
        Player.self.Melt();
        foreach (EntityBase eb in GetActorComponents<EntityBase>())
            eb.Melt();
        foreach (Bullet b in GetActorComponents<Bullet>())
            b.Melt();
    }

    // Called when player steps on portal
    public void FreezeActors()
    {
        Player.self.Freeze();
        foreach (EntityBase eb in GetActorComponents<EntityBase>())
            eb.Freeze();
        foreach (Bullet b in GetActorComponents<Bullet>())
            b.Freeze();
    }

    // Called when player left the stage
    public void Unload()
    {
        if (!loaded) return;
        Destroy(actors);
        hints.ResetHints();
        gameObject.SetActive(false);
        loaded = false;
    }

    public T[] GetActorComponents<T>() => actors.GetComponentsInChildren<T>();
}
