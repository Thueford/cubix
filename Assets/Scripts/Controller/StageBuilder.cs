using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    public static StageBuilder self;
    [NotNull] public GameStage StagePrefab;
    [NotNull] public GameObject ActorContainer;
    [NotNull] public GameObject WallContainer;
    [NotNull] public GameObject BossContainer;


    // Start is called before the first frame update
    void Awake() => self = this;
    GameObject getChild(GameObject obj, int index) => index < obj.transform.childCount ? obj.transform.GetChild(index).gameObject : null;
    GameObject getRandomChild(GameObject obj) => obj.transform.childCount > 1 ? getChild(obj, Random.Range(1, obj.transform.childCount)) : null;

    public GameStage Generate(Transform t, int number)
    {
        Vector3 pos = t.position;
        if (!GameState.curStage.isProcedural) pos.z += 40;

        GameStage stage = Instantiate(StagePrefab, pos, Quaternion.identity);
        stage.isProcedural = true;
        stage.number = number;

        if (stage % 10 > 0)
        {
            GameObject spawner = Instantiate(getRandomChild(ActorContainer), stage.actorsBase.transform);
            spawner.SetActive(true);

            GameObject walls = Instantiate(getRandomChild(WallContainer), stage.actorsBase.transform);
            walls.SetActive(true);
        }
        else
        {
            stage.isBoss = true;

            int index = (stage / 10) % BossContainer.transform.childCount;
            Debug.Log("Generating Boss Stage " + index);
            
            GameObject oBoss = Instantiate(getChild(BossContainer, index), stage.actorsBase.transform);
            pos = stage.portal.transform.position;
            pos.y = 0.5f;
            pos.z -= 5;
            oBoss.transform.position = pos;
            oBoss.SetActive(false);
            // EnemyBase boss = oBoss.GetComponent<EnemyBase>();
        }
        return stage;
    }
}
