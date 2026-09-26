using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RoleSelectorUI : MonoBehaviour
{
    public static RoleSelectorUI Instance { get; private set; }

    [SerializeField] private GameObject lockObj;
    [SerializeField] private CanvasGroup canvasGroup;

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

        player.Role.OnValueChanged += OnRoleChanged;
        otherPlayer.Role.OnValueChanged += OnRoleChanged;

        lobbyPanel.SetActive(true);
        background[0].SetActive(true);
        background[1].SetActive(true);

        UpdateStartButton();
    }

    private void OnRoleChanged(PlayerRole previousValue, PlayerRole newValue)
    {
        UpdateStartButton();
    }

    private void UpdateStartButton()
    {
        bool bothready = player.Role.Value != PlayerRole.None
            && otherPlayer.Role.Value != PlayerRole.None;

        lockObj.SetActive(!bothready);
        
        canvasGroup.alpha = bothready ? 1 : 0.4f;
        canvasGroup.interactable = bothready;
        canvasGroup.blocksRaycasts = bothready;
    }

    public void SwapBox(GameObject mainObject, bool selected)
    {
        mainObject.transform.Find("ConfirmButton").gameObject.SetActive(!selected);
        mainObject.transform.Find("CancelButton").gameObject.SetActive(selected);
    }

    public void SetRoleTextBox(GameObject mainObject, bool selected, string text = "")
    {
        TextMeshProUGUI textObject = mainObject.transform.Find("SelectedText").GetComponent<TextMeshProUGUI>();
        textObject.text = text;
        textObject.gameObject.SetActive(selected);
        
        GameObject confirmButton = mainObject.transform.Find("ConfirmButton").gameObject;
        confirmButton.SetActive(!selected);
    }

    public void CommonActions(ulong playerId, PlayerRole playerRole, SelectionRolType selectionRolType)
    {
        Player player = MapManager.Instance.GetPlayerById(playerId);
        Player otherPlayer = MapManager.Instance.GetOtherPlayer(player);
        
        bool isSelectionType = selectionRolType == SelectionRolType.Select;

        GameObject playerBox = lobbyPanel.transform.Find($"{playerRole}Box").gameObject;
        TextMeshProUGUI playerRoleText = playerBox.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        playerRoleText.text = isSelectionType ? player.name : "Sin escoger";

        InfoBoxText(selectionRolType, player.name, otherPlayer);
    }

    public void InfoBoxText(SelectionRolType selectionRolType, string playerName, Player otherPlayer)
    {
        TextMeshProUGUI infoboxText = lobbyPanel.transform.Find("InfoBox").GetComponentInChildren<TextMeshProUGUI>();
        
        switch (selectionRolType)
        {
            case SelectionRolType.Select:
                if (otherPlayer.Role.Value == PlayerRole.None)
                    infoboxText.text = $"{otherPlayer.name} sin rol";
                else infoboxText.text = "Jugadores listos para empezar";
                break;
            case SelectionRolType.Deselect:
                if (otherPlayer.Role.Value == PlayerRole.None)
                    infoboxText.text += $"\n{playerName} sin rol";
                else infoboxText.text = $"{playerName} sin rol";
                break;
        }
    }

    public void StartGame()
    {
        player.StartGameServerRpc();
    }
    
    public void CloseLobby()
    {
        lobbyPanel.SetActive(false);
        background[0].SetActive(false);
        background[1].SetActive(false);
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
        player.DeselectRoleServerRpc(PlayerRole.Gardener);
    }
    public void DeselectBuilder()
    {
        player.DeselectRoleServerRpc(PlayerRole.Builder);
    }
}
