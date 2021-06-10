using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStage : MonoBehaviour
{
    public GameObject cam, spawn, actors, next;
    public int colorSlots = 3;
    public float chargeTime = 10;

    public void Start()
    {
        gameObject.SetActive(false);
    }

    // Called when player steps on portal
    public void OnStageEntering()
    {
        gameObject.SetActive(true);
        GetComponentInChildren<Portal>().Disable();
    }

    // Called when player is spawning
    public void OnStageEnter()
    {
        Debug.Log("Stage: " + name);
        gameObject.SetActive(true);
        GetComponentInChildren<Portal>().Disable();

        // copy camera
        GameCamera gcam = FindObjectOfType<GameCamera>();
        gcam.target = cam.transform.position;
        gcam.transform.rotation = cam.transform.rotation;

        Hint h = GetComponentInChildren<Hint>();
        if (h != null) h.ResetHints();
        GetComponentInChildren<ChargeAnim>().Reset(1/chargeTime);
        GetComponentInChildren<Spawn>().Disable();
        if(next != null) next.GetComponentInChildren<Spawn>().Enable();

        // actors.SetActive(true);
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
