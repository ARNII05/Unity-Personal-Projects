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
    }

    void Start()
    {
        rabbitDialogue.SetActive(false);
    }

    public void Init(Player player)
    {
        this.player = player;
        rabbitMapUI.Init(player);
    }

    public void OpenDialogue()
    {
        player.SetState(PlayerState.RabbitDialogueInteraction);
        rabbitDialogue.SetActive(true);
    }

    public void CloseDialogue()
    {
        player.SetState(PlayerState.Normal);
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
