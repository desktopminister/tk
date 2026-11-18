using System;
using System.Collections.Generic;
using UnityEngine;

public class TKGameAudio : MonoBehaviour
{
    public enum Cue { Deploy, Bow, FireOrb, Blast, Hit, Hurt, Gem, LevelUp, Boss, Trigram, Lightning, FireBreath, FireArrow }

    [Serializable]
    class Effect
    {
        public Cue cue;
        public AudioClip clip;
        [Range(0f, 1f)] public float gain = 1f;
        [Min(0f)] public float cooldown;
        [NonSerialized] public double readyAt;
    }

    [Header("Music and ambience")]
    [SerializeField] AudioClip title, battle, victory, rain;
    [Range(0f, 1f)] [SerializeField] float musicVolume = 0.65f;
    [Header("Effects")]
    [Range(0f, 1f)] [SerializeField] float effectsVolume = 0.75f;
    [SerializeField] Effect[] effects;

    static TKGameAudio current;
    readonly Dictionary<Cue, Effect> clips = new Dictionary<Cue, Effect>();
    AudioSource score, outgoing, oneShots;
    float trackGain;
    bool onBattlefield, finishing;

    void Awake()
    {
        current = this;
        score = NewSource();
        outgoing = NewSource();
        oneShots = NewSource();
        foreach (var effect in effects)
            clips.Add(effect.cue, effect);
    }

    AudioSource NewSource()
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        source.volume = 0f;
        return source;
    }

    void OnDestroy()
    {
        if (current == this) current = null;
    }

    void ChangeTrack(AudioClip clip, float gain)
    {
        (score, outgoing) = (outgoing, score);
        score.Stop();
        score.clip = clip;
        score.volume = 0f;
        score.loop = true;
        trackGain = gain;
        score.Play();
    }

    public void PlayTitle() => ChangeTrack(title, 0.82f);

    public static void BeginBattle()
    {
        if (!current) return;
        current.onBattlefield = true;
        current.ChangeTrack(current.battle, 0.56f);
        Play(Cue.Deploy);
    }

    public static void EndBattle()
    {
        if (!current) return;
        current.finishing = true;
    }

    public void PlayEnding(bool won)
    {
        if (won) ChangeTrack(victory, 0.58f);
        else ChangeTrack(rain, 0.86f);
    }

    public static void Play(Cue cue)
    {
        if (!current || current.finishing || !current.clips.TryGetValue(cue, out var effect) || !effect.clip) return;
        double now = AudioSettings.dspTime;
        if (now < effect.readyAt) return;
        effect.readyAt = now + effect.cooldown;
        current.oneShots.volume = current.effectsVolume;
        current.oneShots.PlayOneShot(effect.clip, effect.gain);
    }

    void Update()
    {
        float gain = finishing ? 0f : trackGain;
        if (onBattlefield && GameManager.instance && !GameManager.instance.isLive)
            gain *= 0.48f;
        float step = Time.unscaledDeltaTime * 1.8f;
        score.volume = Mathf.MoveTowards(score.volume, musicVolume * gain, step);
        if (finishing)
            oneShots.volume = Mathf.MoveTowards(oneShots.volume, 0f, step);
        if (outgoing.isPlaying)
        {
            outgoing.volume = Mathf.MoveTowards(outgoing.volume, 0f, step);
            if (outgoing.volume == 0f) outgoing.Stop();
        }
    }
}
