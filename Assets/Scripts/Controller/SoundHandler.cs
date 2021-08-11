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

    private static float wHitTimer = 0, eHitTimer = 0, shootTimer = 0, explTimer = 0;

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

}
