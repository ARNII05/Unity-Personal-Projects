using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource sfxAudioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        sfxAudioSource = GameObject.Find("SFXSounds").GetComponent<AudioSource>();
    }

    public void PlaySFXSound(AudioClip audioClip)
    {
        sfxAudioSource.PlayOneShot(audioClip);
    }
}
