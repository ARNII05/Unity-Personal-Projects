using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RoleSelectorUI : MonoBehaviour
{
    public static RoleSelectorUI Instance { get; private set; }

    public GameObject[] background;
    private GameObject lobbyPanel;
    public GameObject gardenerRoleObject;
    public GameObject builderRoleObject;

    Player player;
    Player otherPlayer;
    
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
        lobbyPanel = transform.Find("LobbyPanel").gameObject;
        lobbyPanel.SetActive(false);
        background[0].SetActive(false);
        background[1].SetActive(false);
    }

    public void InitRoleSelector()
    {
        player = NetworkManager.Singleton.LocalClient.PlayerObject
            .GetComponent<Player>();

        otherPlayer = MapManager.Instance.GetOtherPlayer(player);

        lobbyPanel.SetActive(true);
        background[0].SetActive(true);
        background[1].SetActive(true);
    }

    public void SwapBox(GameObject mainObject)
    {
        mainObject.transform.Find("ConfirmButton").gameObject.SetActive(false);
        mainObject.transform.Find("CancelButton").gameObject.SetActive(true);
    }

    public void SetRoleTextBox(string text, GameObject mainObject)
    {
        TextMeshProUGUI textObject = mainObject.transform.Find("SelectedText").GetComponent<TextMeshProUGUI>();
        textObject.text = text;
        textObject.gameObject.SetActive(true);
        
        GameObject builderConfirmObject = mainObject.transform.Find("ConfirmButton").gameObject;
        builderConfirmObject.SetActive(false);
    }

    public void CommonActions(ulong playerId, PlayerRole playerRole)
    {
        Player player = MapManager.Instance.GetPlayerById(playerId);
        Player otherPlayer = MapManager.Instance.GetOtherPlayer(player);

        GameObject playerBox = lobbyPanel.transform.Find($"{playerRole}Box").gameObject;
        TextMeshProUGUI playerRoleText = playerBox.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        playerRoleText.text = player.name;

        TextMeshProUGUI infoboxText = lobbyPanel.transform.Find("InfoBox").GetComponentInChildren<TextMeshProUGUI>();
        infoboxText.text = $"{otherPlayer.name} sin rol";
    }

    public void SetRoleOnUIOnSameId()
    {
        
    }

    public void StartGame()
    {
        if (player.Role.Value != PlayerRole.None
            && otherPlayer.Role.Value != PlayerRole.None)
        {
            lobbyPanel.SetActive(false);
        }
    }

    public void SelectGardener()
    {
        player.SelectRoleServerRpc(PlayerRole.Gardener);
    }

    public void SelectBuilder()
    {
        player.SelectRoleServerRpc(PlayerRole.Builder);
    }

    public void DeselectGardener()
    {

    }
    public void DeselectBuilder()
    {

    }
}
