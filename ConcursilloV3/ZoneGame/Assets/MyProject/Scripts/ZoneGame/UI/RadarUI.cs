using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class RadarUI : MonoBehaviour
{
    public static RadarUI Instance { get; private set; }

    [SerializeField] private GameObject radarDevice;
    [SerializeField] private int distance;
    
    private AudioSource radarAudioSource;
    private VideoPlayer videoPlayer;
    private TextMeshProUGUI radarText;

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

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
        radarAudioSource.Play();
        vp.prepareCompleted -= OnVideoPrepared;
    }

    public void ToggleRadar(Player player, Vector2Int wolfPosition)
    {
        if (radarDevice.activeSelf)
            OnClose(player);
        else
            OnOpen(player, wolfPosition);
    }

    private void OnOpen(Player player, Vector2Int wolfPosition)
    {
        player.State = PlayerState.UsingRadar;

        UpdateRadar(player.NetworkMapPos.Value, wolfPosition);

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

    private void OnClose(Player player)
    {
        radarDevice.SetActive(false);

        videoPlayer.Pause();
        radarAudioSource.Pause();

        player.State = PlayerState.Normal;
    }

    public void UpdateRadar(Vector2Int playerPosition, Vector2Int wolfPosition)
    {
        //int distance = Mathf.Abs(playerPosition.x - wolfPosition.x) + Mathf.Abs(playerPosition.y - wolfPosition.y);

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
