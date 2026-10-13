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
    private TextMeshProUGUI radarText;

    private Player playerWithRadar;

    public Wolf wolf;

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
        radarDevice.SetActive(false);
        radarAudioSource = radarDevice.transform.Find("AudioSource").GetComponent<AudioSource>();
        videoPlayer = radarDevice.transform.Find("ScreenMask").GetComponent<VideoPlayer>();
        radarText = radarDevice.GetComponentInChildren<TextMeshProUGUI>();
        videoPlayer.Prepare();
    }

    public void Init(Player player)
    {
        playerWithRadar = player;
        UpdateRadar(new Vector2Int(0, 0), new Vector2Int(0, 0));

        playerWithRadar.NetworkMapPos.OnValueChanged += UpdateRadar;
        wolf.NetworkPosition.OnValueChanged += UpdateRadar;
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

    private void UpdateRadar(Vector2Int oldPos,
        Vector2Int newPos)
    {
        int distance = Mathf.Abs(playerWithRadar.NetworkMapPos.Value.y - wolf.NetworkPosition.Value.y) 
            + Mathf.Abs(playerWithRadar.NetworkMapPos.Value.x - wolf.NetworkPosition.Value.x);

        radarText.text = distance switch
        {
            <= 1 => "Al acecho!!",
            <= 3 => "Se acerca!",
            _ => "Sin rastro"
        };
        radarText.color = distance switch
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
}
