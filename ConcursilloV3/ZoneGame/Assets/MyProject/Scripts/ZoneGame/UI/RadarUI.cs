using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class RadarUI : MonoBehaviour
{
    public static RadarUI Instance { get; private set; }

    [SerializeField] private GameObject radarDevice;
    
    private AudioSource radarAudioSource;
    private VideoPlayer videoPlayer;
    private TextMeshProUGUI distanceText, wolfStatusText;

    private Player playerWithRadar;

    private Wolf wolf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        wolf = GameObject.Find("WolfManager").GetComponent<Wolf>();

        radarDevice.SetActive(false);
        radarAudioSource = radarDevice.transform.Find("AudioSource").GetComponent<AudioSource>();
        videoPlayer = radarDevice.transform.Find("ScreenMask").GetComponent<VideoPlayer>();
        distanceText = videoPlayer.transform.Find("DistanceText").GetComponent<TextMeshProUGUI>();
        wolfStatusText = videoPlayer.transform.Find("StatusText").GetComponent<TextMeshProUGUI>();
        videoPlayer.Prepare();
    }

    public void Init(Player player)
    {
        playerWithRadar = player;
        UpdateRadar();

        playerWithRadar.NetworkMapPos.OnValueChanged += OnPlayerAndWolfPosChanged;
        wolf.NetworkPosition.OnValueChanged += OnPlayerAndWolfPosChanged;
        wolf.wolfState.OnValueChanged += OnWolfStateChanged;
    }

    private void OnPlayerAndWolfPosChanged(Vector2Int oldPos, Vector2Int newPos)
    {
        UpdateRadar();
    }

    private void OnWolfStateChanged(WolfState oldState, WolfState newState)
    {
        UpdateRadar();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
        radarAudioSource.Play();
        vp.prepareCompleted -= OnVideoPrepared;
    }

    public void ToggleRadar()
    {
        if (radarDevice.activeSelf)
            OnClose();
        else
            OnOpen();
    }

    private void OnOpen()
    {
        playerWithRadar.State = PlayerState.UsingRadar;

        radarDevice.SetActive(true);

        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
            radarAudioSource.Play();
        }
        else
        {
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.Prepare();
        }
    }

    private void OnClose()
    {
        radarDevice.SetActive(false);

        videoPlayer.Pause();
        radarAudioSource.Pause();

        playerWithRadar.State = PlayerState.Normal;
    }

    public void StopVideoAndSound()
    {
        videoPlayer.Pause();
        radarAudioSource.Pause();
    }

    public void PlayVideoAndSound()
    {
        videoPlayer.Play();
        radarAudioSource.Play();
    }

    private void UpdateRadar()
    {
        UpdateDistanceText();
        UpdateStatusText();
    }

    private void UpdateDistanceText()
    {
        int distance = Mathf.Abs(playerWithRadar.NetworkMapPos.Value.y - wolf.NetworkPosition.Value.y)
            + Mathf.Abs(playerWithRadar.NetworkMapPos.Value.x - wolf.NetworkPosition.Value.x);

        distanceText.text = distance switch
        {
            <= 1 => "Al acecho",
            <= 3 => "Cerca",
            _ => "Sin rastro"
        };
        distanceText.color = distance switch
        {
            <= 1 => Color.red,
            <= 3 => Color.yellow,
            _ => Color.white
        };
        videoPlayer.playbackSpeed = distance switch
        {
            <= 1 => 2,
            <= 3 => 1.5f,
            _ => 1
        };
        radarAudioSource.pitch = distance switch
        {
            <= 1 => 2,
            <= 3 => 1.5f,
            _ => 1
        };
    }

    private void UpdateStatusText()
    {
        wolfStatusText.color = Color.white;
        
        switch (wolf.wolfState.Value)
        {
            case WolfState.MovingRandom:
                wolfStatusText.text = "Merodeando";
                break;

            case WolfState.MovingToPlayer:
                wolfStatusText.text = "Persiguiendo a un jugador";
                wolfStatusText.color = distanceText.color;
                break;

            case WolfState.MovingToMeat:
                wolfStatusText.text = "Yendo hacia la carne";
                break;

            case WolfState.MovingToDen:
                wolfStatusText.text = "Regresando a la cueva";
                break;
        }
    }
}
