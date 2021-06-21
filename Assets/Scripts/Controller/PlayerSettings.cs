using System.Collections;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerSettings
{
    public static byte curProfile = 1;


    private const string SAVEDATA_BASE = "player";
    private const string SAVEDATA_EXT = ".save";
    private static string SAVEDATA_DIR { get { return Application.dataPath + "/saves/"; } }

    public static string GetSavePath(byte profile)
    {
        if (!Directory.Exists(SAVEDATA_DIR))
            Directory.CreateDirectory(SAVEDATA_DIR);
        return SAVEDATA_DIR + SAVEDATA_BASE + profile + SAVEDATA_EXT;
    }

    public static void SaveProfile(PlayerSettings s)
    {
        string json = JsonUtility.ToJson(s, true);
        File.WriteAllText(GetSavePath(curProfile), json);
    }

    public static PlayerSettings LoadProfile()
    {
        string filePath = GetSavePath(curProfile);
        if (!File.Exists(filePath)) goto returnDefault;

        string data = File.ReadAllText(filePath);
        if (string.IsNullOrEmpty(data)) goto returnDefault;

        return JsonUtility.FromJson<PlayerSettings>(data);

    returnDefault:
        Debug.LogErrorFormat("{0} not found. Loading default profile", filePath);
        return new PlayerSettings();
    }
}