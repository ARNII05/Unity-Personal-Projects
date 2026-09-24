using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsManager : MonoBehaviour
{
    private const float defaultVolume = 1f;

    AudioSource source;

    AudioClip[] onFireSounds;

    private HashSet<int> onFireIndexes = new();

    [Header("Sonidos de Interfaz")]
    public AudioClip electionSound;
    public AudioClip teamKill;

    [Header("Sonidos de Gameplay")]
    public AudioClip roundFinished;
    public AudioClip payload;

    public static SoundEffectsManager Instance { get; private set; }

    public static SoundEffectsManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        GameObject soundObject = new("ConcursilloV2SoundEffectsManager");
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = defaultVolume;

        return soundObject.AddComponent<SoundEffectsManager>();
    }

    public static void DestroyInstance()
    {
        if (Instance == null) return;

        Destroy(Instance.gameObject);
        Instance = null;
    }

    private void Awake()
    {
        source = GetComponent<AudioSource>();

        if (Instance != null && Instance != this)
        {
            Instance.CopyMissingClipsFrom(this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Initialize()
    {
        if (source == null)
            source = GetComponent<AudioSource>();

        AudioSettingsManager.RegisterAudioSource(source, AudioChannel.Sfx);
        LoadMissingClips();
    }

    private void LoadMissingClips()
    {
        if (electionSound == null)
            electionSound = Resources.Load<AudioClip>("ConcursilloV2/SoundEffects/electionSound");

        if (teamKill == null)
            teamKill = Resources.Load<AudioClip>("ConcursilloV2/SoundEffects/teamKill");

        if (roundFinished == null)
            roundFinished = Resources.Load<AudioClip>("ConcursilloV2/SoundEffects/roundFinished");

        if (payload == null)
            payload = Resources.Load<AudioClip>("ConcursilloV2/SoundEffects/Payload");

        if (onFireSounds == null || onFireSounds.Length == 0)
            onFireSounds = Resources.LoadAll<AudioClip>("ConcursilloV2/SoundEffects/OnFire");
    }

    public AudioClip OnFireRandomClip()
    {
        Initialize();

        if (onFireSounds == null || onFireSounds.Length == 0)
        {
            Debug.LogWarning("Could not play on-fire sound because no clips were found in Resources/ConcursilloV2/SoundEffects/OnFire.");
            return null;
        }

        int index;
        
        do
        {
            index = Random.Range(0, onFireSounds.Length);
        }
        while (!onFireIndexes.Add(index));

        if (onFireIndexes.Count == onFireSounds.Length)
            onFireIndexes.Clear();

        return onFireSounds[index];
    }

    public void PlaySound(AudioClip clip)
    {
        Initialize();

        if (clip != null)
        {
            source.PlayOneShot(clip);
        }
    }

    private void CopyMissingClipsFrom(SoundEffectsManager other)
    {
        if (other == null) return;

        if (electionSound == null) electionSound = other.electionSound;
        if (teamKill == null) teamKill = other.teamKill;
        if (roundFinished == null) roundFinished = other.roundFinished;
        if (payload == null) payload = other.payload;
    }
}
