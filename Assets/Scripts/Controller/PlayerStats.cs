using System.Collections;
using System.IO;
using System.IO.Compression;
using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    private byte profile = 2;
    public static bool saveZipped = false;
    public static System.Text.Encoding enc = System.Text.Encoding.UTF8;

    public double totalTime = 0;
    public double tutorialTime = 0;
    public bool reachedEndless = false;
    public string colorOrder = "";
    public int[] stageDeaths = new int[11];

    public int startNo = 0;
    public int stageHighscore = 0;

    public int totalDeaths = 0;
    public int totalKills = 0;
    public int totalClears = 0;
    public int sacrifices = 0;
    public int colHalo = 0, colInvis = 0, colAtk = 0, colEnd = 0;

    public int endlessClears = 0;
    public int tutorialDeaths = 0;
    public int tutorialClears = 0;

    public PlayerStats(byte profile)
    {
        this.profile = profile;
    }

    public void Save()
    {
        Save(this);
    }

    public object[] getStartStats()
    {
        object[] stats = { 
            System.TimeSpan.FromSeconds(totalTime),
            System.TimeSpan.FromSeconds(tutorialTime),
            stageHighscore,
            totalKills, 
            totalDeaths };
        return stats;
    }

    public object[] getMoreStats()
    {
        object[] stats = {
            startNo,
            endlessClears,
            tutorialDeaths,
            sacrifices };
        return stats;
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

    public static void Save(PlayerStats s)
    {
        string json = JsonUtility.ToJson(s, !saveZipped);
        byte[] data = { };
        
        if (saveZipped) data = ZipStr(json);
        if (data.Length == 0) data = enc.GetBytes(json);

        File.WriteAllBytes(GetSavePath(s.profile), data);
    }

    public static PlayerStats LoadProfile(byte profile)
    {
        string filePath = GetSavePath(profile);
        if (!File.Exists(filePath)) goto returnDefault;

        byte[] data = File.ReadAllBytes(filePath);
        if (data.Length == 0) goto returnDefault;

        string json = enc.GetString(data);
        if (data[0] != '{') json = UnZipStr(data);
        if (string.IsNullOrEmpty(json)) goto returnDefault;

        PlayerStats p = JsonUtility.FromJson<PlayerStats>(json);
        p.profile = profile;
        return p;

    returnDefault:
        Debug.LogWarningFormat("{0} not found. Loading default profile", filePath);
        return new PlayerStats(profile);
    }

    public static byte[] ZipStr(string str)
    {
        using (MemoryStream output = new MemoryStream())
        {
            using (DeflateStream gzip = new DeflateStream(output, CompressionMode.Compress))
            using (StreamWriter writer = new StreamWriter(gzip, enc))
                writer.Write(str);
            return output.ToArray();
        }
    }

    public static string UnZipStr(byte[] input)
    {
        try
        {
            using (MemoryStream inputStream = new MemoryStream(input))
            using (DeflateStream gzip = new DeflateStream(inputStream, CompressionMode.Decompress))
            using (StreamReader reader = new StreamReader(gzip, enc))
                return reader.ReadToEnd();
        }
        catch (IOException) { return null; }
    }
    #endregion
}