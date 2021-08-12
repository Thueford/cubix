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
    int randomChildIndex(GameObject obj) => Random.Range(1, obj.transform.childCount);
    
    public GameStage Generate(Transform t, int number)
    {
        Vector3 pos = t.position;
        if (!GameState.curStage.info.isProcedural) pos.z += 40;

        GameStage stage = Instantiate(StagePrefab, pos, Quaternion.identity);
        stage.info.isProcedural = true;
        stage.info.stageNo = stage.number = number;

        if (stage % 5 > 0)
        {
            stage.info.spawnerId = randomChildIndex(ActorContainer);
            GameObject spawner = Instantiate(getChild(ActorContainer, stage.info.spawnerId), stage.actorsBase.transform);
            spawner.SetActive(true);

            stage.info.wallId = randomChildIndex(WallContainer);
            GameObject walls = Instantiate(getChild(WallContainer, stage.info.wallId), stage.actorsBase.transform);
            walls.SetActive(true);
        }
        else
        {
            stage.info.isBoss = true;

            stage.info.bossId = (stage / 5) % BossContainer.transform.childCount;
            Debug.Log("Generating Boss Stage " + stage.info.bossId);

            GameObject oBoss = Instantiate(getChild(BossContainer, stage.info.bossId), stage.actorsBase.transform);
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
