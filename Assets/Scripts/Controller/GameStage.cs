using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStage : MonoBehaviour
{
    public GameObject cam, spawn, next;

    public void OnStageEnter()
    {
        Debug.Log("Stage: " + name);
        GetComponentInChildren<Hint>().ResetHints();
        GetComponentInChildren<ChargeAnim>().Reset();
    }

    public void OnStageExit()
    {
        GetComponentInChildren<Portal>().Disable();
    }
}
