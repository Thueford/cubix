using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStage : MonoBehaviour
{
    [NotNull] public GameObject actors;
    [NotNull] public Camera cam;
    [WarnNull] public GameStage next;
    [NotNull] public Spawn spawn;
    [NotNull] public Portal portal;
    [WarnNull] public Hint hints;

    public int colorSlots = 3;
    public float chargeTime = 10;

    public void Start()
    {
        gameObject.SetActive(false);
    }

    // Called when player steps on portal
    public void OnStageEntering()
    {
        Debug.Log("Entering");
        gameObject.SetActive(true);
        portal.Disable();
        if (hints != null) hints.ResetHints();
    }

    // Called when player is spawning
    public void OnStageEnter()
    {
        Debug.Log("Enter");
        Debug.Log("Stage: " + name);

        // copy camera
        Camera.main.GetComponent<GameCamera>().target = cam.transform.position;
        Camera.main.transform.rotation = cam.transform.rotation;

        GetComponentInChildren<ChargeAnim>().ResetAnim(chargeTime);
        spawn.Disable();
        if(next != null) next.spawn.Enable();

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
