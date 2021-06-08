using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStage : MonoBehaviour
{
    public GameObject cam, spawn, actors, next;

    public void Start()
    {
        actors.SetActive(false);
    }

    // Called when player steps on portal
    public void OnStageEntering()
    {

    }

    // Called when player is spawning
    public void OnStageEnter()
    {
        Debug.Log("Stage: " + name);

        // copy camera
        GameCamera gcam = FindObjectOfType<GameCamera>();
        gcam.target = cam.transform.position;
        gcam.transform.rotation = cam.transform.rotation;

        Hint h = GetComponentInChildren<Hint>();
        if (h != null) h.ResetHints();
        GetComponentInChildren<ChargeAnim>().Reset();
        GetComponentInChildren<Spawn>().Disable();
        if(next != null) next.GetComponentInChildren<Spawn>().Enable();

        actors.SetActive(true);
    }

    public void OnStageEntered()
    {
        foreach (EnemyBase eb in GetComponentsInChildren<EnemyBase>())
            eb.movable = true;
        foreach (ShooterBase sb in GetComponentsInChildren<ShooterBase>())
            sb.active = true;
    }

    // Called when player steps on portal
    public void OnStageExit()
    {
        foreach (EnemyBase eb in GetComponentsInChildren<EnemyBase>())
            eb.movable = false;
    }

    // Called when player left the stage
    public void OnStageExited()
    {
        GetComponentInChildren<Portal>().Disable();
        actors.SetActive(false);
    }
}
