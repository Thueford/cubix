using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Collectable : MonoBehaviour
{
    private static HaloShooter hs;
    private static GameObject CollPrefab;
    private static float chance = 0.1f;
    private static float chanceWhite = 0.075f;

    private Particles ps;
    private Light l;
    private Renderer r;
    private Animator anim;

    [NotNull] public Explosion explosion;

    public enum cType
    {
        RED, GREEN, BLUE,
        HALO, INVIS, ATKSPD,
        ENDALLEXISTENCE,
        BOSS,
        BLACK // should always be last (StageStats)
    }

    public cType type;

    private static Dictionary<Vector3Int, cType> v2Type = new Dictionary<Vector3Int, cType>()
    {
        { Vector3Int.one, cType.ENDALLEXISTENCE },
        { new Vector3Int(1, 1, 0), cType.HALO },
        { new Vector3Int(1, 0, 1), cType.INVIS },
        { new Vector3Int(0, 1, 1), cType.ATKSPD },
        { Vector3Int.right, cType.RED },
        { Vector3Int.up, cType.GREEN },
        { Vector3Int.forward, cType.BLUE },
        { Vector3Int.zero, cType.BLACK }
    };

    private void Awake()
    {
        ps = GetComponentInChildren<Particles>();
        l = GetComponentInChildren<Light>();
        r = GetComponentInChildren<Renderer>();
        anim = GetComponent<Animator>();
        anim.keepAnimatorControllerStateOnDisable = true;
        if (!CollPrefab) CollPrefab = gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!hs) hs = Player.self.GetComponent<HaloShooter>();
        if (!hs) Debug.LogError("HaloShooter not found");
        PlayerConfig.self.setLight(l);
        setType(type);
        StageStats.cur.addCollectable(this);
    }

    private void Update()
    {
        if (type == cType.BOSS)
        {
            setColor(new Color(
                Mathf.Sin(Time.time) / 2 + 0.5f,
                Mathf.Sin(Time.time + 4/3 * Mathf.PI) / 2 + 0.5f,
                Mathf.Sin(Time.time + 2/3 * Mathf.PI) / 2 + 0.5f
                ), true);
        }
    }

    public void setType(cType t)
    {
        type = t;
        switch (type)
        {
            case cType.RED: setColor(GameState.red); break;
            case cType.GREEN: setColor(GameState.green); break;
            case cType.BLUE: setColor(GameState.blue); break;
            case cType.HALO: setColor(Color.yellow); PlayerStats.self.colHalo++; break;
            case cType.INVIS: setColor(Color.magenta); PlayerStats.self.colInvis++; break;
            case cType.ATKSPD: setColor(Color.cyan); PlayerStats.self.colAtk++; break;
            case cType.ENDALLEXISTENCE: setColor(Color.white); PlayerStats.self.colEnd++; break;
            default: setColor(GameState.black); break;
        }
    }

    public static void Clear(GameStage s)
    {
        foreach (Collectable c in s.actors.GetComponentsInChildren<Collectable>()) c.Kill();
    }

    private void setColor(Color color, bool ignoreColorHelper = false)
    {
        ps.color.color = GameState.getLightColor(color);

        if (ignoreColorHelper) 
        {
            r.material.color = l.color = ps.color.color2 = color;
            return;
        }

        if (color != Color.white)
        {
            r.material.color = color;
            l.color = GameState.getLightColor(color);
            ps.color.color2 = (Color.white + 0.3f * GameState.getLightColor(color)) / 1.3f;
        }
        else
        {
            r.material.color = Color.Lerp(GameState.gold, color, .5f);
            l.color = Color.Lerp(GameState.gold, color, .5f);
            l.intensity = 5;
            ps.color.color2 = GameState.gold;
        }
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
        l.enabled = false;
        ps.SetEnabled(false);
        GetComponent<Animator>().Play("Die");
    }

    public void OnDie() { Destroy(gameObject); }

    private void OnCollect()
    {
        Debug.Log("collected " + nameof(type));

        switch (type)
        {
            case cType.BLACK:
                Player.self.Hit(-1);
                break;
            case cType.RED:
                GameState.addRed();
                Ressource.self.addRes(Ressource.col.Red, 50);
                ResParts.Spawn(transform.position, 50, ps.color.color);
                break;
            case cType.GREEN:
                GameState.addGreen();
                Ressource.self.addRes(Ressource.col.Green, 50);
                ResParts.Spawn(transform.position, 50, ps.color.color);
                break;
            case cType.BLUE:
                GameState.addBlue();
                Ressource.self.addRes(Ressource.col.Blue, 50);
                ResParts.Spawn(transform.position, 50, ps.color.color);
                break;
            case cType.HALO:
                hs.activate(5);
                break;
            case cType.INVIS:
                Player.self.MakeInvulnurable(5f);
                break;
            case cType.ATKSPD:
                Player.self.bs.atkSpeedBoost(5f, 2f);
                break;
            case cType.ENDALLEXISTENCE:
                //Time.timeScale = 0.5f;
                Instantiate(explosion, GameState.curStage.transform.position, Quaternion.identity)
                    .SetProperties("Player", 30, 10, 0.8f);
                break;
            case cType.BOSS:
                if (Random.value <= 1 / 3)
                {
                    Player.self.bs.atkSpeedBoost(1.2f);
                    GUIManager.self.showTip("+ Attack Speed");
                }
                else if (Random.value <= 2 / 3)
                {
                    Player.self.maxSpeed *= 1.2f;
                    GUIManager.self.showTip("+ Movement Speed");
                }
                else
                {
                    Player.self.maxHP += 1;
                    GUIManager.self.showTip("+1 Max HP");
                }
                break;
            default:
                break;
        }
        if (type != cType.ENDALLEXISTENCE) SoundHandler.PlayClip("collect");
    }

    public static void Drop(Vector3Int col, Vector3 pos, bool boss = false)
    {
        if (boss) 
        {
            Debug.Log("dropped " + nameof(type));
            Instantiate(CollPrefab, pos, Quaternion.identity, GameState.curStage.actors.transform).
                GetComponent<Collectable>().setType(cType.BOSS);
            return;
        }

        if (!GameState.IsEndless()) return;

        float dropChance = chance;
        if (col == Vector3Int.one || col == Vector3Int.zero)
            dropChance = chanceWhite;
        else
        {
            dropChance *= col.sqrMagnitude;
            if (col.z == 1) dropChance /= 3;
        }

        if (Random.value <= dropChance)
        {
            Debug.Log("dropped " + nameof(type));
            Instantiate(CollPrefab, pos, Quaternion.identity, GameState.curStage.actors.transform).
                GetComponent<Collectable>().setType(v2Type[col]);
        }
    }
}
