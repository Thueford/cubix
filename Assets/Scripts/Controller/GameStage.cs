using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStage : MonoBehaviour
{
    public GameObject cam, spawn, next;

    public void OnBeforeStageEnter()
    {

    }

    public void OnStageEnter()
    {
        Debug.Log("Stage: " + name);
        Hint h = GetComponentInChildren<Hint>();
        if (h != null) h.ResetHints();
        GetComponentInChildren<ChargeAnim>().Reset();
        GetComponentInChildren<Spawn>().Disable();
        if(next != null) next.GetComponentInChildren<Spawn>().Enable();
    }

    public void OnStageExit()
    {
        GetComponentInChildren<Portal>().Disable();
    }
}
