using System.IO;
using System.IO.Compression;
using UnityEngine;

[System.Serializable]
public abstract class SaveData
{
    public byte profile;
    public const int saveVersion = 1;

    [System.Serializable]
    public struct Data
    {
        public byte profile;
        public PlayerConfig config;
        public PlayerStats stats;
        public StageStats stags;

        public Data Load(byte profile)
        {
            this.profile = profile;
            Debug.Log("Loading profile " + profile);
            config = LoadProfile<PlayerConfig>(profile, ".config");
            stats = LoadProfile<PlayerStats>(profile, ".stats");
            stags = LoadProfile<StageStats>(profile, ".stags"); // will always create new
            return this;
        }

        public void Save(byte profile = 0)
        {
            SaveStats();
            config.Save();
        }

        public void SaveStats(byte profile = 0)
        {
            if (profile != 0) this.profile = profile;
            stats.Save();
            stags.Save(stats.startNo);
        }
    }

    #region statics

    public static bool saveZipped = false;
    protected static string dataDir = null;
    protected static System.Text.Encoding enc = System.Text.Encoding.UTF8;
    protected static string SAVEDATA_DIR => dataDir + "/saves/";
    private const string SAVEDATA_BASE = "player";

    public static void SaveProfile<T>(T save, byte profile, string ext) where T : SaveData
    {
        string json = JsonUtility.ToJson(save).Replace("{", "\n{");
        byte[] data = { };

        if (saveZipped) data = ZipStr(json);
        if (data.Length == 0) data = enc.GetBytes(json);

        File.WriteAllBytes(GetSavePath(profile, ext), data);
    }

    private static bool tryMakeDir(string path)
    {
        try
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return true;
        }
        catch (System.Exception) { }
        return false;
    }
    public static string GetSavePath(byte profile, string ext)
    {
        if (dataDir == null) dataDir = Application.dataPath;
        if (!tryMakeDir(SAVEDATA_DIR))
        {
            dataDir = Application.persistentDataPath;
            tryMakeDir(SAVEDATA_DIR);
        }
        return SAVEDATA_DIR + SAVEDATA_BASE + profile + ext;
    }

    private static T LoadProfile<T>(byte profile, string ext) where T : SaveData, new()
    {
        T p;

        string filePath = GetSavePath(profile, ext);
        if (!File.Exists(filePath)) goto returnDefault;

        byte[] data = File.ReadAllBytes(filePath);
        if (data.Length == 0) goto returnDefault;

        string json = enc.GetString(data);
        if (data[0] != '{') json = UnZipStr(data);
        if (string.IsNullOrEmpty(json)) goto returnDefault;

        p = JsonUtility.FromJson<T>(json);
        p.profile = profile;
        return p;

    returnDefault:
        Debug.LogWarningFormat("{0} not found. Loading default profile", filePath);
        p = new T();
        p.profile = profile;
        return p;
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