using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject transitionCanvas;
    [SerializeField] private VideoPlayer transitionVideo;

    [SerializeField] private TextMeshProUGUI playerPosition;
    [SerializeField] private TextMeshProUGUI zoneName;

    private Action onTransitionCovered;

    [SerializeField] private float zoneChangeDelay = 0.5f;

    Player player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        transitionVideo.loopPointReached += OnTransitionFinished;
        playerPosition.gameObject.SetActive(false);
        zoneName.gameObject.SetActive(false);
    }
    public void Init()
    {
        playerPosition.gameObject.SetActive(true);
        zoneName.gameObject.SetActive(true);
    }
    
    public void UpdatePlayerUI(Vector2Int position, string zone)
    {
        playerPosition.text = $"X: {position.x + 1} Y: {position.y + 1}";
        zoneName.text = zone;
    }

    public void PlayZoneTransition(Action onCovered, Player player)
    {
        this.player = player;

        player.State = PlayerState.Transitioning;
        
        onTransitionCovered = onCovered;

        transitionCanvas.SetActive(true);

        transitionVideo.Stop();
        transitionVideo.Play();

        StartCoroutine(WaitForTransition());
    }

    private IEnumerator WaitForTransition()
    {
        yield return new WaitForSeconds(zoneChangeDelay);

        if (onTransitionCovered != null)
        {
            Action action = onTransitionCovered;
            onTransitionCovered = null;

            action.Invoke();
        }
    }

    private void OnTransitionFinished(VideoPlayer videoPlayer)
    {
        transitionCanvas.SetActive(false);

        player.State = PlayerState.Normal;
    }

    private void OnDestroy()
    {
        if (transitionVideo != null)
            transitionVideo.loopPointReached -= OnTransitionFinished;
    }
}