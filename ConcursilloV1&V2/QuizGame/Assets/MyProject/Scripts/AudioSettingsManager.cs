using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum AudioChannel
{
    Music,
    Game,
    Sfx
}

public class AudioSettingsManager : MonoBehaviour
{
    private const string MusicKey = "Audio.MusicVolume";
    private const string GameKey = "Audio.GameVolume";
    private const string SfxKey = "Audio.SfxVolume";

    private static AudioSettingsManager instance;

    private readonly Dictionary<AudioSource, float> baseVolumes = new();
    private readonly Dictionary<AudioSource, AudioChannel> channels = new();

    private Canvas runtimeCanvas;
    private GameObject runtimePanel;
    private Slider runtimeMusicSlider;
    private Slider runtimeGameSlider;
    private Slider runtimeSfxSlider;
    private Font runtimeFont;

    public static AudioSettingsManager Instance
    {
        get
        {
            if (instance == null)
                CreateInstance();

            return instance;
        }
    }

    public float MusicVolume { get; private set; } = 1f;
    public float GameVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    public event Action VolumesChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        CreateInstance();
    }

    private static void CreateInstance()
    {
        if (instance != null) return;

        var managerObject = new GameObject("AudioSettingsManager");
        instance = managerObject.AddComponent<AudioSettingsManager>();
        DontDestroyOnLoad(managerObject);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumes();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || SceneHasOptionsPanel())
            return;

        ToggleRuntimePanel();
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = ClampVolume(volume);
        SaveAndApply();
    }

    public void SetGameVolume(float volume)
    {
        GameVolume = ClampVolume(volume);
        SaveAndApply();
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = ClampVolume(volume);
        SaveAndApply();
    }

    public static void RegisterAudioSource(AudioSource source, AudioChannel channel)
    {
        Instance.Register(source, channel);
    }

    public static float GetAppliedVolume(AudioSource source, AudioChannel channel)
    {
        return Instance.GetAppliedSourceVolume(source, channel);
    }

    public static void SetAppliedVolume(AudioSource source, AudioChannel channel, float volume)
    {
        Instance.SetAppliedSourceVolume(source, channel, volume);
    }

    private void Register(AudioSource source, AudioChannel channel)
    {
        if (source == null) return;

        if (!baseVolumes.ContainsKey(source))
            baseVolumes[source] = source.volume;

        channels[source] = channel;
        InitializeChannelVolume(channel, source.volume);
        ApplyVolume(source);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterSceneAudioSources();

        if (runtimePanel != null && SceneHasOptionsPanel())
            runtimePanel.SetActive(false);
    }

    private void RegisterSceneAudioSources()
    {
        foreach (AudioSource source in Resources.FindObjectsOfTypeAll<AudioSource>())
        {
            if (source == null || !source.gameObject.scene.isLoaded)
                continue;

            bool hasKnownAudioManager = source.GetComponent<MusicManager>() != null
                || source.GetComponent<SoundEffectsManager>() != null;

            if (source.clip == null && !hasKnownAudioManager)
                continue;

            Register(source, GuessChannel(source));
        }
    }

    private AudioChannel GuessChannel(AudioSource source)
    {
        if (source.GetComponent<SoundEffectsManager>() != null)
            return AudioChannel.Sfx;

        if (source.GetComponent<MusicManager>() != null)
            return AudioChannel.Music;

        string objectName = source.gameObject.name;
        if (objectName.IndexOf("Sound", StringComparison.OrdinalIgnoreCase) >= 0)
            return AudioChannel.Sfx;

        if (objectName.IndexOf("AudioController", StringComparison.OrdinalIgnoreCase) >= 0)
            return AudioChannel.Game;

        return AudioChannel.Music;
    }

    private void ApplyAllVolumes()
    {
        List<AudioSource> deadSources = null;

        foreach (AudioSource source in baseVolumes.Keys)
        {
            if (source == null)
            {
                deadSources ??= new List<AudioSource>();
                deadSources.Add(source);
                continue;
            }

            ApplyVolume(source);
        }

        if (deadSources == null) return;

        foreach (AudioSource source in deadSources)
        {
            baseVolumes.Remove(source);
            channels.Remove(source);
        }
    }

    private void ApplyVolume(AudioSource source)
    {
        if (source == null || !baseVolumes.ContainsKey(source))
            return;

        AudioChannel channel = channels.TryGetValue(source, out AudioChannel savedChannel)
            ? savedChannel
            : GuessChannel(source);

        source.volume = GetChannelVolume(channel);
    }

    private float GetAppliedSourceVolume(AudioSource source, AudioChannel channel)
    {
        Register(source, channel);

        return source != null ? source.volume : GetChannelVolume(channel);
    }

    private void SetAppliedSourceVolume(AudioSource source, AudioChannel channel, float volume)
    {
        Register(source, channel);
        SetChannelVolume(channel, volume);
    }

    private float GetChannelVolume(AudioChannel channel)
    {
        float volume = channel switch
        {
            AudioChannel.Game => GameVolume,
            AudioChannel.Sfx => SfxVolume,
            _ => MusicVolume
        };

        return volume >= 0f ? volume : 1f;
    }

    private void SetChannelVolume(AudioChannel channel, float volume)
    {
        switch (channel)
        {
            case AudioChannel.Game:
                SetGameVolume(volume);
                break;
            case AudioChannel.Sfx:
                SetSfxVolume(volume);
                break;
            default:
                SetMusicVolume(volume);
                break;
        }
    }

    private void LoadVolumes()
    {
        MusicVolume = PlayerPrefs.HasKey(MusicKey) ? PlayerPrefs.GetFloat(MusicKey) : -1f;
        GameVolume = PlayerPrefs.HasKey(GameKey) ? PlayerPrefs.GetFloat(GameKey) : -1f;
        SfxVolume = PlayerPrefs.HasKey(SfxKey) ? PlayerPrefs.GetFloat(SfxKey) : -1f;
    }

    private void SaveAndApply()
    {
        SaveVolume(MusicKey, MusicVolume);
        SaveVolume(GameKey, GameVolume);
        SaveVolume(SfxKey, SfxVolume);
        PlayerPrefs.Save();

        ApplyAllVolumes();
        VolumesChanged?.Invoke();
        SyncRuntimePanel();
    }

    private float ClampVolume(float volume)
    {
        return Mathf.Clamp01(volume);
    }

    private void SaveVolume(string key, float volume)
    {
        if (volume >= 0f)
            PlayerPrefs.SetFloat(key, ClampVolume(volume));
        else
            PlayerPrefs.DeleteKey(key);
    }

    private bool HasChannelVolume(AudioChannel channel)
    {
        return channel switch
        {
            AudioChannel.Game => GameVolume >= 0f,
            AudioChannel.Sfx => SfxVolume >= 0f,
            _ => MusicVolume >= 0f
        };
    }

    private void InitializeChannelVolume(AudioChannel channel, float volume)
    {
        if (HasChannelVolume(channel)) return;

        switch (channel)
        {
            case AudioChannel.Game:
                GameVolume = ClampVolume(volume);
                break;
            case AudioChannel.Sfx:
                SfxVolume = ClampVolume(volume);
                break;
            default:
                MusicVolume = ClampVolume(volume);
                break;
        }
    }

    private bool SceneHasOptionsPanel()
    {
        foreach (Options options in Resources.FindObjectsOfTypeAll<Options>())
        {
            if (options != null && options.gameObject.scene.isLoaded)
                return true;
        }

        return false;
    }

    private void ToggleRuntimePanel()
    {
        EnsureRuntimePanel();
        EnsureEventSystem();
        runtimePanel.SetActive(!runtimePanel.activeSelf);
        SyncRuntimePanel();
    }

    private void EnsureRuntimePanel()
    {
        if (runtimePanel != null) return;

        EnsureEventSystem();

        GameObject canvasObject = new GameObject("RuntimeOptionsCanvas");
        DontDestroyOnLoad(canvasObject);

        runtimeCanvas = canvasObject.AddComponent<Canvas>();
        runtimeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        runtimeCanvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        runtimePanel = new GameObject("RuntimeOptionsPanel");
        runtimePanel.transform.SetParent(canvasObject.transform, false);

        Image panelImage = runtimePanel.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.02f, 0.02f, 0.68f);

        RectTransform panelRect = runtimePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(620f, 320f);

        CreateText("Opciones", new Vector2(0f, 112f), new Vector2(500f, 44f), 34, TextAnchor.MiddleCenter);

        runtimeMusicSlider = CreateSlider("Musica", 50f, SetMusicVolume);
        runtimeGameSlider = CreateSlider("Juego", -30f, SetGameVolume);
        runtimeSfxSlider = CreateSlider("Efectos", -110f, SetSfxVolume);

        runtimePanel.SetActive(false);
    }

    private Slider CreateSlider(string label, float y, Action<float> onChanged)
    {
        CreateText(label, new Vector2(-200f, y), new Vector2(160f, 38f), 28, TextAnchor.MiddleLeft);

        GameObject sliderObject = new GameObject(label + " Slider");
        sliderObject.transform.SetParent(runtimePanel.transform, false);

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.onValueChanged.AddListener(value => onChanged(value));

        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(110f, y);
        sliderRect.sizeDelta = new Vector2(350f, 26f);

        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObject.transform, false);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.34f, 0.29f, 0.17f, 1f);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = new Vector2(0f, 7f);
        backgroundRect.offsetMax = new Vector2(0f, -7f);
        slider.targetGraphic = backgroundImage;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(0f, 7f);
        fillAreaRect.offsetMax = new Vector2(0f, -7f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.89f, 0.68f, 0.18f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        slider.fillRect = fillRect;

        GameObject handleSlideArea = new GameObject("Handle Slide Area");
        handleSlideArea.transform.SetParent(sliderObject.transform, false);
        RectTransform handleSlideAreaRect = handleSlideArea.AddComponent<RectTransform>();
        handleSlideAreaRect.anchorMin = Vector2.zero;
        handleSlideAreaRect.anchorMax = Vector2.one;
        handleSlideAreaRect.offsetMin = new Vector2(10f, 0f);
        handleSlideAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleSlideArea.transform, false);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(0.98f, 0.92f, 0.68f, 1f);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(22f, 38f);
        slider.handleRect = handleRect;

        return slider;
    }

    private Text CreateText(string text, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(text);
        textObject.transform.SetParent(runtimePanel.transform, false);

        Text labelText = textObject.AddComponent<Text>();
        labelText.text = text;
        labelText.font = GetRuntimeFont();
        labelText.fontSize = fontSize;
        labelText.color = Color.white;
        labelText.alignment = alignment;

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        return labelText;
    }

    private Font GetRuntimeFont()
    {
        if (runtimeFont == null)
            runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return runtimeFont;
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void SyncRuntimePanel()
    {
        if (runtimePanel == null) return;

        SetSliderValueWithoutNotify(runtimeMusicSlider, GetChannelVolume(AudioChannel.Music));
        SetSliderValueWithoutNotify(runtimeGameSlider, GetChannelVolume(AudioChannel.Game));
        SetSliderValueWithoutNotify(runtimeSfxSlider, GetChannelVolume(AudioChannel.Sfx));
    }

    private void SetSliderValueWithoutNotify(Slider slider, float value)
    {
        if (slider == null) return;

        slider.SetValueWithoutNotify(value);
    }
}
