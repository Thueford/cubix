using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    public static SoundHandler self;

    [NotNull] public AudioSource MusicSource;
    [NotNull] public AudioSource EffectSource;
    [NotNull] public AudioSource MenuSource;

    [NotNull] public AudioHighPassFilter filterHigh;
    [NotNull] public AudioLowPassFilter filterLow;

    [Range(0, 1)]
    public float volume = 1;

    [Serializable]
    public struct NamedClip
    {
        public string name;
        public AudioClip[] clip;
    }

    public NamedClip[] clipArray;

    public static Dictionary<string, AudioClip[]> clips = new Dictionary<string, AudioClip[]>();

    private static bool hpRunning = false, lpRunning = false;

    private static float wHitTimer = 0, eHitTimer = 0, shootTimer = 0, explTimer = 0;

    private static float filterHighValue = 10, filterLowValue = 22000;

    // Start is called before the first frame update
    void Awake()
    {
        if (!self) self = this;
        foreach (NamedClip c in clipArray)
        {
            if (clips.ContainsKey(c.name))
                throw new ArgumentException("Two same Audio Clip Names");
            else 
                clips.Add(c.name, c.clip);
        }
    }

    // Update is called once per frame
    void Update()
    {
        wHitTimer += Time.deltaTime;
        eHitTimer += Time.deltaTime;
        shootTimer += Time.deltaTime;
        explTimer += Time.deltaTime;

        if (filterHigh.cutoffFrequency != filterHighValue && !GameState.paused)
        {
            float delta = filterHigh.cutoffFrequency - filterHighValue;
            if (Mathf.Abs(delta) < 50)
                filterHigh.cutoffFrequency = filterHighValue;
            else
                filterHigh.cutoffFrequency -= delta * 0.3f;
        }

        if (filterLow.cutoffFrequency != filterLowValue && !GameState.paused)
        {
            float delta = filterLow.cutoffFrequency - filterLowValue;
            if (Mathf.Abs(delta) < 50)
                filterLow.cutoffFrequency = filterLowValue;
            else
                filterLow.cutoffFrequency -= delta * 0.3f;
        }
    }

    public static void PlayClip(AudioClip c)
    {
        self.EffectSource.PlayOneShot(c);
    }
    public static void PlayClip(AudioClip[] c)
    {
        if (c == clips["wallHit"])
        {
            if (wHitTimer < 0.08) return;
            wHitTimer = 0;
        }
        else if (c == clips["enemyHit"])
        {
            if (eHitTimer < 0.1) return;
            eHitTimer = 0;
        }
        else if (c == clips["shoot"])
        {
            if (shootTimer < 0.1) return;
            shootTimer = 0;
        }
        else if (c == clips["explosion"])
        {
            if (explTimer < 0.1) return;
            explTimer = 0;
        }
        PlayClip(c[UnityEngine.Random.Range(0, c.Length)]);
    }

    public static void SetVolume(float volMusic, float volEffects)
    {
        self.MusicSource.volume = volMusic;
        self.EffectSource.volume = volEffects;
        self.MenuSource.volume = volEffects;
    }

    public static void PlayClip(string s) => PlayClip(clips[s]);

    public static void PlayClick()
    {
        self.MenuSource.PlayOneShot(clips["click"][0]);
    }

    public static void SetHPTarget(float frequency) => filterHighValue = frequency;
    public static void SetLPTarget(float frequency) => filterLowValue = frequency;

    public static void OnPause(bool pause)
    {
        if (pause)
        {
            self.filterLow.cutoffFrequency = 2000;
            self.filterHigh.cutoffFrequency = 10;
        }
        else
        {
            self.filterLow.cutoffFrequency = filterLowValue;
            self.filterHigh.cutoffFrequency = filterHighValue;
        }
    }

    public static void SetLPTarget(float frequency, float duration)
    {
        if (!lpRunning) self.StartCoroutine(E_StartLP(frequency, duration));
    }

    public static void SetHPTarget(float frequency, float duration)
    {
        if (!hpRunning) self.StartCoroutine(E_StartHP(frequency, duration));
    }

    private static IEnumerator E_StartLP(float frequency, float duration)
    {
        lpRunning = true;
        float oldVal = filterLowValue;
        filterLowValue = frequency;
        while (duration > 0)
        {
            if (!GameState.paused) duration -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        filterLowValue = oldVal;
        lpRunning = false;
    }

    private static IEnumerator E_StartHP(float frequency, float duration)
    {
        hpRunning = true;
        float oldVal = filterHighValue;
        filterHighValue = frequency;
        while (duration > 0)
        {
            if (!GameState.paused) duration -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        filterHighValue = oldVal;
        hpRunning = false;
    }
}
