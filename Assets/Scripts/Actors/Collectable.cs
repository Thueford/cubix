using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Collectable : MonoBehaviour
{
    private static HaloShooter hs;
    [NotNull] public Explosion explosion;

    public enum cType
    {
        RED,
        GREEN,
        BLUE,
        HALO, 
        INVIS,
        ATKSPD,
        ENDALLEXISTENCE
    }

    public cType type;

    // Start is called before the first frame update
    void Start()
    {
        setType(type);
    }

    private void Awake()
    {
        hs = Player.self.GetComponent<HaloShooter>();
        if (!hs) Debug.LogError("HaloShooter not found");
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
            case cType.RED: setColor(GameState.red); break;
            case cType.GREEN: setColor(GameState.green); break;
            case cType.BLUE: setColor(GameState.blue); break;
            default: setColor(GameState.black); break;
        }
    }

    public static void Clear(GameStage s)
    {
        foreach (Collectable c in s.actors.GetComponentsInChildren<Collectable>()) c.Kill();
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
            Kill();
            OnCollect();
        }
    }

    public void Kill()
    {
        GetComponent<SphereCollider>().enabled = false;
        gameObject.GetComponentInChildren<Light>().enabled = false;
        gameObject.GetComponent<Particles>().SetEnabled(false);
        GetComponent<Animator>().Play("Die");
    }

    public void OnDie() { Destroy(gameObject); }

    private void OnCollect()
    {
        switch (type)
        {
            case cType.RED:
                GameState.addRed();
                Ressource.self.addRes(Ressource.col.Red, 50);
                break;
            case cType.GREEN:
                GameState.addGreen();
                Ressource.self.addRes(Ressource.col.Green, 50);
                break;
            case cType.BLUE:
                GameState.addBlue();
                Ressource.self.addRes(Ressource.col.Blue, 50);
                break;
            case cType.HALO:
                hs.activate(5);
                break;
            case cType.INVIS:
                Player.self.MakeInvulnurable(5f);
                break;
            case cType.ATKSPD:
                //Player.self.bs.atkSpd;
                break;
            case cType.ENDALLEXISTENCE:
                Instantiate(explosion, transform.position, Quaternion.identity)
                    .SetProperties("Player", 20, 10);
                break;
            default:
                break;
        }
    }
}
