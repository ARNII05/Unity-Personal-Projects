using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private const string basePath = "ConcursilloV2/Music";
    private const float defaultVolume = 0.035f;

    AudioSource musicSource;

    private AudioClip[] musicClips;
    private AudioClip previousMusic;

    private readonly HashSet<int> playedMusicIndexes = new();

    private int musicLength;
    private int lastPlayedIndex = -1;

    private float previousTime;

    bool wasPlaying = false;
    bool manuallyStopped = false;

    public static MusicManager Instance { get; private set; }

    public static MusicManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        GameObject musicObject = new("ConcursilloV2MusicController");
        AudioSource source = musicObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.volume = defaultVolume;

        return musicObject.AddComponent<MusicManager>();
    }

    public static void DestroyInstance()
    {
        if (Instance == null) return;

        Destroy(Instance.gameObject);
        Instance = null;
    }

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (Instance != this) return;

        Initialize();
        PlayMusic();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Initialize()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        AudioSettingsManager.RegisterAudioSource(musicSource, AudioChannel.Music);

        if (musicClips != null && musicClips.Length > 0) return;

        musicClips = Resources.LoadAll<AudioClip>(basePath);
        musicLength = musicClips.Length;
        SelectMusic();
    }
    
    private void Update()
    {
        if (musicSource == null || musicClips == null) return;

        if (musicSource.isPlaying)
        {
            wasPlaying = true;
        }
        else if (wasPlaying && !manuallyStopped)
        {
            wasPlaying = false;
            SelectMusic();
            musicSource.Play();
        }
    }

    public void PlayMusic()
    {
        Initialize();

        if (musicSource.clip == null || musicSource.isPlaying) return;

        musicSource.Play();
    }
    
    public void PlayMusic(AudioClip clip)
    {
        Initialize();

        if (clip == null || musicSource.clip == clip) return;

        manuallyStopped = true;
        wasPlaying = false;

        previousMusic = musicSource.clip;
        previousTime = musicSource.time;
        
        musicSource.Stop();
        
        musicSource.clip = clip;
        musicSource.Play();
    }
        
    public void StopMusic()
    {
        Initialize();

        if (musicSource.clip == null || musicSource.clip.name != "OvertimeSound") return;

        manuallyStopped = true;
        wasPlaying = false;

        musicSource.Stop();
        
        musicSource.clip = previousMusic;
        musicSource.time = previousTime;

        musicSource.Play();
        manuallyStopped = false;
    }

    void SelectMusic()
    {
        if (musicClips == null || musicClips.Length == 0) return;

        if (playedMusicIndexes.Count == musicClips.Length)
            playedMusicIndexes.Clear();

        if (musicClips.Length == 1)
        {
            musicSource.clip = musicClips[0];
            lastPlayedIndex = 0;
            playedMusicIndexes.Add(0);
            return;
        }

        int index;
        do
        {
            index = Random.Range(0, musicLength);
        }
        while (index == lastPlayedIndex || !playedMusicIndexes.Add(index));

        musicSource.clip = musicClips[index];
        lastPlayedIndex = index;
    }
}
