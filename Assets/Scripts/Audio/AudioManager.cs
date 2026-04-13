using System.Collections.Generic;
using UnityEngine;

public enum SFX
{
    Fire,
    Bomb,

    PressStart,

    Count
}

public enum BGMTrack
{
    Title,
    GamePlay,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public SFX sfx;
        public AudioClip clip;
        public float volume = 1f;
        public float pitch = 1f;
    }

    public float BGMVolume = 0.5f;
    public AudioClip[] BGMClips;
    public Sound[] soundEffects;

    private Dictionary<SFX, Sound> sfxDictionary;
    private AudioSource sfxAudioSource;

    private AudioSource bgmAudioSource;
    

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmAudioSource = gameObject.AddComponent<AudioSource>();

        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxDictionary = new Dictionary<SFX, Sound>();

        foreach (var s in soundEffects)
        {
            if (!sfxDictionary.ContainsKey(s.sfx))
                sfxDictionary.Add(s.sfx, s);
        }
    }

    public void PlayBGM(BGMTrack track)
    {
        bgmAudioSource.Stop();

        bgmAudioSource.clip = BGMClips[(int)track];
        bgmAudioSource.volume = BGMVolume;
        bgmAudioSource.loop = true;
        bgmAudioSource.Play();
    }

    public void StopBGM()
    {
        bgmAudioSource.Stop();
    }

    public void PlaySFX(SFX sfx)
    {
        if (sfxDictionary.ContainsKey(sfx))
        {
            Sound s = sfxDictionary[sfx];
            sfxAudioSource.pitch = s.pitch;
            sfxAudioSource.PlayOneShot(s.clip, s.volume);
        }
        else
        {
            Debug.LogWarning($"SFX '{name}' not found!");
        }
    }
}
