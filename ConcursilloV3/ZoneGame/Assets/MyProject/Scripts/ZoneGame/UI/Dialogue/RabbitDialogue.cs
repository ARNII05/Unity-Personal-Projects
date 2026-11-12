using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitDialogue : MonoBehaviour
{
    public static RabbitDialogue Instance { get; private set; }

    private GameObject rabbitDialogue;

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

        rabbitDialogue = transform.Find("RabbitDialogue").gameObject;
        rabbitMapUI = GameObject.Find("RabbitPanel").GetComponent<RabbitMapUI>();
        MapManager.Instance.InitRabbitMapUI(rabbitMapUI);
    }

    void Start()
    {
        rabbitDialogue.SetActive(false);
    }

    public void Init(Player player)
    {
        this.player = player;
        rabbitMapUI.InitMap(player);
    }

    public void OpenDialogue()
    {
        player.SetNewStateRpc(PlayerState.RabbitDialogueInteraction);
        rabbitDialogue.SetActive(true);
    }

    public void CloseDialogue()
    {
        player.SetNewStateRpc(PlayerState.Normal);
        rabbitDialogue.SetActive(false);
    }
    
    public void ShowMap()
    {
        rabbitDialogue.SetActive(false);
        rabbitMapUI.ToggleRabbitMap();
    }

    public void CloseMap()
    {
        rabbitMapUI.ToggleRabbitMap();
    }
}
