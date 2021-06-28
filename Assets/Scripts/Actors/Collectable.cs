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
            case cType.Red: setColor(GameState.red); break;
            case cType.Green: setColor(GameState.green); break;
            case cType.Blue: setColor(GameState.blue); break;
            default: setColor(GameState.black); break;
        }
    }

    public static void Clear(GameStage s)
    {
        foreach (Collectable coll in s.actors.GetComponentsInChildren<Collectable>())
            Destroy(coll.gameObject);
    }

    private void setColor(Color color)
    {
        gameObject.GetComponentInChildren<Renderer>().material.color = color;
        gameObject.GetComponentInChildren<Light>().color = GameState.getLightColor(color);

        Particles ps = gameObject.GetComponent<Particles>();
        ps.color.color = GameState.getLightColor(color);
        ps.color.color2 = (Color.white + 0.3f * GameState.getLightColor(color)) / 1.3f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnCollect();
            gameObject.GetComponentInChildren<Light>().enabled = false;
            gameObject.GetComponent<Particles>().SetEnabled(false);
            GetComponent<Animator>().Play("Die");
        }
    }

    public void OnDie() { Destroy(gameObject); }

    private void OnCollect()
    {
        switch (type)
        {
            case cType.Red:
                GameState.addRed();
                Ressource.self.addRes(Ressource.col.Red, 50);
                break;
            case cType.Green:
                GameState.addGreen();
                Ressource.self.addRes(Ressource.col.Green, 50);
                break;
            case cType.Blue:
                GameState.addBlue();
                Ressource.self.addRes(Ressource.col.Blue, 50);
                break;
            default:
                break;
        }
    }
}
