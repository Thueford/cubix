using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameStage : MonoBehaviour
{
    [NotNull] public Camera cam;
    [NotNull] public Spawn spawn;
    [NotNull] public Charger charger;
    [NotNull] public Portal portal;
    public GameObject actors;
    [NotNull] public GameObject actorsBase;
    [WarnNull] public GameStage next;
    [WarnNull] public StageController hints;

    [Range(0,  3)] public int colorSlots = 3;
    [Range(1,100)] public float chargeTime = 10;
    [Range(1,100)] public int maxEnemies = 10;

    public int number;
    public bool isProcedural = false;

    public void Awake()
    {
        gameObject.SetActive(false);
        actorsBase.SetActive(false);
    }

    // Reset entities
    public void ResetStage()
    {
        /*
        foreach (EnemySpawner eb in GetActorComponents<EnemySpawner>())
            eb.ResetSpawner();
        */

        if (actors) Destroy(actors);
        portal.SetEnabled(false);
        if (hints) hints.ResetHints();
        charger.gameObject.SetActive(false);
        charger.gameObject.SetActive(true);
    }

    // Called when player steps on portal
    public void Load()
    {
        gameObject.SetActive(true);

        actors = Instantiate(actorsBase, gameObject.transform);
        //actors.SetActive(false);
        portal.SetEnabled(false);

        if (hints != null) hints.ResetHints();
    }

    // Called when player is spawning
    public void OnStageEnter()
    {
        Debug.Log("Stage: " + name);

        // copy camera
        Camera.main.GetComponent<GameCamera>().target = cam.transform.position;
        Camera.main.transform.rotation = cam.transform.rotation;

        charger.Reset(chargeTime);
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
        ResetStage();
        gameObject.SetActive(false);
    }

    public T[] GetActorComponents<T>() { return actors.GetComponentsInChildren<T>(); }
}
