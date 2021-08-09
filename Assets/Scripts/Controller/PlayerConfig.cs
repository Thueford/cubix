using UnityEngine;

[System.Serializable]
public class PlayerConfig : SaveData
{
    public string version;

    public float volMusic = 1;
    public float volSoundEff = 1;

    public int quality = 0;
    public bool shader = true;
    public bool computes = true;
    public bool particles = true;
    public bool lights = true;
    public bool postProc = true;

    public bool lensFlare = true;
    public bool bloom = true;
    public bool CTREffect = true;
    public bool CAEffect = true;

    public PlayerConfig()
    {
        version = Application.version + ":" + saveVersion;
    }

    public void setQuality(int quality)
    {
        if (quality >= QualitySettings.names.Length) return;
        QualitySettings.SetQualityLevel(quality, true);
        this.quality = quality;
    }

    public void setLight(Light l) => l.enabled = lights;
    public void setLights()
    {
        Player.self.GetComponentInChildren<Light>().enabled = lights;
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("Stage"))
            foreach (Light l in o.GetComponentsInChildren<Light>())
                l.enabled = lights;
    }

    public void UpdateSettings()
    {
        if (!SystemInfo.supportsComputeShaders) computes = false;

        bool _cmps = shader && computes;
        Particles.enableParticles = _cmps && particles;
        PostProcessing.self.skipAll = !shader || !postProc;

        PostProcessing.self.useBloom = _cmps && bloom;
        PostProcessing.self.useLensFlare = _cmps && lensFlare;
        PostProcessing.self.useCA = CAEffect;
        PostProcessing.self.useCRTEffect = CTREffect;

        setLight(Player.self.GetComponentInChildren<Light>());
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("Stage"))
            foreach (Light l in o.GetComponentsInChildren<Light>()) setLight(l);
    }

    public void ReadConfigShortcuts()
    {
        if (!InputHandler.ReadCtl()) return;

        InputHandler.ResetLast();
        if (InputHandler.ReadKeyDown(KeyCode.Alpha0)) setQuality(0);
        if (InputHandler.ReadKeyDown(KeyCode.Alpha1)) setQuality(1);
        if (InputHandler.ReadKeyDown(KeyCode.Alpha2)) setQuality(2);
        if (InputHandler.ReadKeyDown(KeyCode.Alpha3)) setQuality(3);
        if (InputHandler.ReadKeyDown(KeyCode.Alpha4)) setQuality(4);
        if (InputHandler.ReadKeyDown(KeyCode.Alpha5)) setQuality(5);

        if (InputHandler.ReadKeyDown(KeyCode.M)) GameState.showMenu = !GameState.showMenu;
        if (InputHandler.ReadKeyDown(KeyCode.S)) shader = !shader;
        if (InputHandler.ReadKeyDown(KeyCode.P)) particles = !particles;
        if (InputHandler.ReadKeyDown(KeyCode.O)) postProc = !postProc;
        if (InputHandler.ReadKeyDown(KeyCode.C)) computes = !computes;

        if (InputHandler.ReadKeyDown(KeyCode.B)) bloom = !bloom;
        if (InputHandler.ReadKeyDown(KeyCode.F)) lensFlare = !lensFlare;
        if (InputHandler.ReadKeyDown(KeyCode.A)) CAEffect = !CAEffect;
        if (InputHandler.ReadKeyDown(KeyCode.R)) CTREffect = !CTREffect;
        if (InputHandler.ReadKeyDown(KeyCode.L)) lights = !lights;

        if (InputHandler.GetLast() != -1)
        {
            UpdateSettings();
            Save();
        }
    }

    public void ConfigMenu()
    {
        GUIStyle style = new GUIStyle();
        style.normal.background = GameState.self.menuBackground;

        GUILayout.BeginHorizontal();
        GUILayout.Space(100);

        GUILayout.BeginVertical();
        GUILayout.Space(100);

        GUILayout.BeginHorizontal(style);
        GUILayout.Space(20);
        GUILayout.BeginVertical();
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Quality: ");
        string[] names = QualitySettings.names;
        for (int i = 0; i < QualitySettings.names.Length; i++)
            if (GUILayout.Button(names[i]))
                QualitySettings.SetQualityLevel(i, true);
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.Label("Sounds: ");
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();

        GUILayout.Label("Music Volume");
        volMusic = GUILayout.HorizontalSlider(volMusic, 0, 1);

        GUILayout.Label("SoundEffect Volume");
        volSoundEff = GUILayout.HorizontalSlider(volSoundEff, 0, 1);

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.Label("Visuals: ");
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();

        shader = GUILayout.Toggle(shader, " Shader   [X + S]");
        particles = GUILayout.Toggle(particles, " Particles   [X + P]");
        postProc = GUILayout.Toggle(postProc, " PostProcessing   [X + O]");
        computes = GUILayout.Toggle(computes, " Computes   [X + C]");

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.Label("PostProcessing Effects: ");
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();

        bloom = GUILayout.Toggle(bloom, " Bloom   [X + B]");
        lensFlare = GUILayout.Toggle(lensFlare, " LensFlare   [X + F]");
        CAEffect = GUILayout.Toggle(CAEffect, " Chromatic Abberation   [X + A]");
        CTREffect = GUILayout.Toggle(CTREffect, " CTR   [X + R]");
        lights = GUILayout.Toggle(lights, " Lights   [X + L]");

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Logs")) System.Diagnostics.Process.Start(dataDir);
        if (GUILayout.Button("Config")) System.Diagnostics.Process.Start(SAVEDATA_DIR);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        UpdateSettings();
        if (Input.GetMouseButtonDown(0)) Save();
    }

    public void Save()
    {
        Debug.Log("Saving config");
        SaveProfile(this, profile, ".config");
    }
}