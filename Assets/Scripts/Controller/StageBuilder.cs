using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    public static StageBuilder self;
    [NotNull] public GameStage StagePrefab;
    [WarnNull] public GameObject ActorContainer;
    [WarnNull] public GameObject WallContainer;


    // Start is called before the first frame update
    void Awake() => self = this;
    GameObject getRandomChild(GameObject obj) => obj.transform.childCount > 1 ? obj.transform.GetChild(Random.Range(1, obj.transform.childCount)).gameObject : null;

    public GameStage Generate(Transform t)
    {
        Vector3 pos = t.position;
        if (!GameState.curStage.isProcedural) pos.z += 40;

        GameStage stage = Instantiate(StagePrefab, pos, Quaternion.identity);
        stage.isProcedural = true;
        stage.number = GameState.curStage.number + 1;
        
        GameObject spawner = Instantiate(getRandomChild(ActorContainer), stage.actorsBase.transform);
        spawner.SetActive(true);

        GameObject walls = Instantiate(getRandomChild(WallContainer), stage.actorsBase.transform);
        walls.SetActive(true);

        return stage;
    }
}
