using System.Collections;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerSettings
{
    private byte profile = 1;

    public int startNo = 0;
    public int stageHighscore = 0;
    public bool reachedEndless = false;

    public PlayerSettings(byte profile)
    {
        this.profile = profile;
    }

    public void Save()
    {
        Save(this);
    }


    #region statics

    private static string SAVEDATA_DIR { get { return Application.dataPath + "/saves/"; } }
    private const string SAVEDATA_BASE = "player";
    private const string SAVEDATA_EXT = ".save";

    public static string GetSavePath(byte profile)
    {
        if (!Directory.Exists(SAVEDATA_DIR))
            Directory.CreateDirectory(SAVEDATA_DIR);
        return SAVEDATA_DIR + SAVEDATA_BASE + profile + SAVEDATA_EXT;
    }

    public static void Save(PlayerSettings s)
    {
        string json = JsonUtility.ToJson(s, true);
        File.WriteAllText(GetSavePath(s.profile), json);
    }

    public static PlayerSettings LoadProfile(byte profile)
    {
        string filePath = GetSavePath(profile);
        if (!File.Exists(filePath)) goto returnDefault;

        string data = File.ReadAllText(filePath);
        if (string.IsNullOrEmpty(data)) goto returnDefault;

        PlayerSettings p = JsonUtility.FromJson<PlayerSettings>(data);
        p.profile = profile;
        return p;

    returnDefault:
        Debug.LogWarningFormat("{0} not found. Loading default profile", filePath);
        return new PlayerSettings(profile);
    }
    #endregion
}