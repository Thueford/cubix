using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    public static StageBuilder self;
    [NotNull] public GameObject StagePrefab;
    [WarnNull] public GameObject ActorContainer;
    [WarnNull] public GameObject WallContainer;
    private List<GameObject> WallBases;


    // Start is called before the first frame update
    void Start()
    {
        if (!self) self = this;
        //ActorBases = GetAllChildren(ActorContainer);
        WallBases = GetAllChildren(WallContainer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Source: https://answers.unity.com/questions/594210/get-all-children-gameobjects.html
    public static List<GameObject> GetAllChildren(GameObject Go)
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < Go.transform.childCount; i++)
        {
            list.Add(Go.transform.GetChild(i).gameObject);
        }
        return list;
    }

    public GameStage Generate(Transform t)
    {
        Vector3 pos = t.position;
        if (!GameState.curStage.isProcedural) pos.z += 40;
        GameObject Stage = Instantiate(StagePrefab, pos, Quaternion.identity);
        GameStage StageScript = Stage.GetComponent<GameStage>();
        StageScript.isProcedural = true;
        StageScript.actorsBase = Instantiate(
            ActorContainer.transform.GetChild(Random.Range(0, ActorContainer.transform.childCount)).gameObject, 
            StageScript.actorsBase.transform.position, 
            Quaternion.identity, 
            Stage.transform);
        /* StageScript.wallBase = Instantiate(
            WallBases[Random.Range(0, WallBases.Count)],
            StageScript.wallBase.transform.position,
            Quaternion.identity,
            Stage.transform); */
        return StageScript;
    }
}
