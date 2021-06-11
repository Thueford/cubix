using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Collectable : MonoBehaviour
{

    public enum cType
    {
        Red,
        Green,
        Blue
    }

    public cType type;

    // Start is called before the first frame update
    void Start()
    {
        setType(type);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setType(cType t)
    {
        type = t;
        switch (type)
        {
            case cType.Red:
                setColor(GameState.red);
                break;
            case cType.Green:
                setColor(GameState.green);
                break;
            case cType.Blue:
                setColor(GameState.blue);
                break;
            default:
                setColor(GameState.black);
                break;
        }
    }

    private void setColor(Color color)
    {
        gameObject.GetComponentInChildren<Renderer>().material.color = color;
        gameObject.GetComponentInChildren<Light>().color = color == GameState.black ? GameState.glow : color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnCollect();
            //Animation
            Destroy(gameObject);
        }
    }

    private void OnCollect()
    {
        switch (type)
        {
            case cType.Red:
                GameState.addRed();
                break;
            case cType.Green:
                GameState.addGreen();
                break;
            case cType.Blue:
                GameState.addBlue();
                break;
            default:
                break;
        }
    }
}
