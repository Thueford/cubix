
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static Helper;

[System.Serializable]
public class StageStats : SaveData
{
    public static StageStat cur = null;
    public List<StageStat> stages = new List<StageStat>();

    public void addStage(int stageno, GameState.State s)
    {
        cur = new StageStat(stageno);
        cur.state = s;
        stages.Add(cur);
    }

    public void Save(int startno)
    {
        if (!PlayerConfig.self.recordStats) return;
        if (cur != null) cur.chargeTime = round(cur.chargeTime);
        Debug.Log("Saving stags");
        SaveProfile(this, profile, $"_{startno}.stags");
    }
}


[System.Serializable]
public class StageStat
{
    public int stageno;
    public GameState.State state;

    #region player
    public float chargeTime = 0;
    public int[] colorShots = new int[8]; // [8] rgb bits (0b000 black, 0b111 white)
    public bool completed = false;        // true when charged
    #endregion

    public int[] collsDropped = new int[(int)Collectable.cType.BLACK];

    #region enemies
    public int enemsKilled = 0;
    public int[] enemyCols = new int[8]; // [8] rgb bits (0b000 black, 0b111 white)
    public int[] enemyTypes = new int[4]; // 0:undefined 1:hunter, 2:archer, 3:stray
    public int maxEnems = 0;
    #endregion

    public StageStat(int stageNo) => stageno = stageNo;

    public void AddShot(Color c) => colorShots[C2B(c)]++;

    private void AddEnemyCol(Color c) => enemyCols[C2B(c)]++;
    private void AddEnemyType(EnemyBase e) => enemyTypes[
        e is E_Hunter ? 1 :
        e is E_Archer ? 2 :
        e is E_Stray ? 3 : 0]++;
    public void AddEnemy(EnemyBase e, int enemCount)
    {
        AddEnemyCol(e._color);
        AddEnemyType(e);
        if (maxEnems < enemCount) maxEnems = enemCount;
    }

    public void addCollectable(Collectable c) => collsDropped[(int)c.type]++;
    public void Charge(Charger c)
    {
        if (c.active && !c.charged) chargeTime += Time.deltaTime;
    }
}
