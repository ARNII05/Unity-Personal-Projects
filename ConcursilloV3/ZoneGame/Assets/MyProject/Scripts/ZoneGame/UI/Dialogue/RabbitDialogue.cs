using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitDialogue : MonoBehaviour
{
    public static RabbitDialogue Instance { get; private set; }

    [SerializeField] private GameObject rabbitPanel;

    private RabbitMapUI rabbitMapUI;

    Player player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rabbitMapUI = GetComponent<RabbitMapUI>();
    }

    void Start()
    {
        rabbitPanel.SetActive(false);
    }

    public void Init(Player player)
    {
        this.player = player;
        rabbitMapUI.InitMap(player);
    }

    public void OpenDialogue()
    {
        player.State = PlayerState.RabbitDialogueInteraction;
        rabbitPanel.SetActive(true);
    }

    public void CloseDialogue()
    {
        player.State = PlayerState.Normal;
        rabbitPanel.SetActive(false);
    }
    
    public void ShowMap()
    {
        rabbitPanel.SetActive(false);
        rabbitMapUI.ToggleRabbitMap();
    }

    public void CloseMap()
    {
        rabbitMapUI.ToggleRabbitMap();
    }
}
