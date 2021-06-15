using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameStage : MonoBehaviour
{
    [NotNull] public Camera cam;
    [NotNull] public Spawn spawn;
    [NotNull] public Portal portal;
    [NotNull] public GameObject actors;
    [WarnNull] public GameStage next;
    [WarnNull] public Hint hints;

    [Range(0,  3)] public int colorSlots = 3;
    [Range(1,100)] public float chargeTime = 10;
    [Range(1,100)] public int maxEnemies = 10;

    public void Start()
    {
        gameObject.SetActive(false);
    }

    // Reset entities
    public void ResetStage()
    {
        foreach (EnemySpawner eb in GetComponentsInChildren<EnemySpawner>())
            eb.ResetSpawner();
    }

    // Called when player steps on portal
    public void OnStageEntering()
    {
        gameObject.SetActive(true);
        actors.SetActive(false);
        portal.Disable();
        if (hints != null) hints.ResetHints();
    }

    // Called when player is spawning
    public void OnStageEnter()
    {
        Debug.Log("Stage: " + name);

        // copy camera
        Camera.main.GetComponent<GameCamera>().target = cam.transform.position;
        Camera.main.transform.rotation = cam.transform.rotation;

        GetComponentInChildren<ChargeAnim>().ResetAnim(chargeTime);
        actors.SetActive(true);
        spawn.Disable();

        //if(next != null) next.spawn.Enable();
    }

    public void OnStageEntered()
    {
        Player.self.Melt();
        foreach (EntityBase eb in GetComponentsInChildren<EntityBase>())
            eb.Melt();
    }

    // Called when player steps on portal
    public void OnStageExit()
    {
        Player.self.Freeze();
        foreach (EntityBase eb in GetComponentsInChildren<EntityBase>())
            eb.Freeze();
    }

    // Called when player left the stage
    public void OnStageExited()
    {
        GetComponentInChildren<Portal>().Disable();
        gameObject.SetActive(false);
    }
}
