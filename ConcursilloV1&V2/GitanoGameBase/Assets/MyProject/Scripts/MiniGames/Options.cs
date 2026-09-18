using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicSlider;
    public Slider gameSlider;
    public Slider soundEffectSlider;

    [Header("Audio Sources")]
    public AudioSource musicAudioSource;
    public AudioSource gameAudioSource;
    public AudioSource soundAudioSource;

    [Header("UI")]
    public GameObject optionsPanel;

    private Slider actualSlider;
    private int actualAudioIndex = -1;

    private Slider draggingSlider;
    private bool syncingSliders;

    void Start()
    {
        ConnectSlider(musicSlider, 0);
        ConnectSlider(gameSlider, 1);
        ConnectSlider(soundEffectSlider, 2);
        SyncSliders();
    }

    void Update()
    {
        if (!IsPanelOpen()) return;

        AudioControl(0, musicSlider);
        AudioControl(1, gameSlider);
        AudioControl(2, soundEffectSlider);
    }

    void AudioControl(int index, Slider slider)
    {
        if (slider == null) return;
        
        if (draggingSlider != slider)
        {
            slider.SetValueWithoutNotify(GetVolume(index));
        }
    }

    public void StartDrag(int index)
    {
        switch (index)
        {
            case 0:
                actualSlider = musicSlider;
                actualAudioIndex = 0;
                draggingSlider = musicSlider;
                break;

            case 1:
                actualSlider = gameSlider;
                actualAudioIndex = 1;
                draggingSlider = gameSlider;
                break;

            case 2:
                actualSlider = soundEffectSlider;
                actualAudioIndex = 2;
                draggingSlider = soundEffectSlider;
                break;
        }
    }

    public void EndDrag()
    {
        if (actualAudioIndex < 0 || actualSlider == null)
        {
            draggingSlider = null;
            return;
        }

        SetVolume(actualAudioIndex, actualSlider.value);

        draggingSlider = null;
        actualSlider = null;
        actualAudioIndex = -1;
    }

    private void SyncSliders()
    {
        syncingSliders = true;

        if (musicSlider != null) musicSlider.SetValueWithoutNotify(GetVolume(0));
        if (gameSlider != null) gameSlider.SetValueWithoutNotify(GetVolume(1));
        if (soundEffectSlider != null) soundEffectSlider.SetValueWithoutNotify(GetVolume(2));

        syncingSliders = false;
    }

    private float GetVolume(int index)
    {
        return index switch
        {
            0 => AudioSettingsManager.GetAppliedVolume(musicAudioSource, AudioChannel.Music),
            1 => AudioSettingsManager.GetAppliedVolume(gameAudioSource, AudioChannel.Game),
            2 => AudioSettingsManager.GetAppliedVolume(soundAudioSource, AudioChannel.Sfx),
            _ => 1f
        };
    }

    private void SetVolume(int index, float value)
    {
        if (syncingSliders) return;

        switch (index)
        {
            case 0:
                AudioSettingsManager.SetAppliedVolume(musicAudioSource, AudioChannel.Music, value);
                break;
            case 1:
                AudioSettingsManager.SetAppliedVolume(gameAudioSource, AudioChannel.Game, value);
                break;
            case 2:
                AudioSettingsManager.SetAppliedVolume(soundAudioSource, AudioChannel.Sfx, value);
                break;
        }
    }

    private bool IsPanelOpen()
    {
        return optionsPanel != null ? optionsPanel.activeSelf : gameObject.activeSelf;
    }

    private void ConnectSlider(Slider slider, int index)
    {
        if (slider == null) return;

        slider.onValueChanged.AddListener(value => SetVolume(index, value));
    }
}
