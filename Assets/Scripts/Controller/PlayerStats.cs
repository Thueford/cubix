
using UnityEngine;

[System.Serializable]
public class PlayerStats : SaveData
{
    #region general
    public double totalTime;
    public int startNo;
    public int stageHighscore;
    public bool reachedEndless = false;

    public long fpsSum;
    public int fpsCount, fpsMin, fpsMax;
    #endregion

    #region player
    public int stageRestarts;

    public int firedBullets;
    public int firedRedShots;
    public int firedGreenShots;
    public int firedBlueShots;

    public int totalDeaths;
    public int totalKills;
    public int totalClears;
    public int sacrifices;
    public int colHalo, colInvis, colAtk, colEnd;

    public int endlessClears;
    #endregion

    #region tutorial
    public string colorOrder = "";
    public double tutorialTime;
    public int tutorialDeaths;
    public int tutorialClears;
    public int[] stageDeaths = new int[11];
    #endregion


    public object[] getStartStats()
    {
        object[] stats = {
            System.TimeSpan.FromSeconds(totalTime),
            startNo,
            stageHighscore,
            firedBullets };
        return stats;
    }
    public object[] getMoreStats()
    {
        object[] stats = {
            endlessClears,
            totalKills,
            totalDeaths,
            sacrifices };
        return stats;
    }

    public void Save()
    {
        Debug.Log("Saving stats");
        SaveProfile(this, profile, ".stats");
    }
}