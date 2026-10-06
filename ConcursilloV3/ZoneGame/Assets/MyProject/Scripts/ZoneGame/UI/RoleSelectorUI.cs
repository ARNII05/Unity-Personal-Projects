using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RoleSelectorUI : MonoBehaviour
{
    public static RoleSelectorUI Instance { get; private set; }

    [SerializeField] private GameObject lockObj, startGameObj;
    [SerializeField] private CanvasGroup canvasGroup;

    public GameObject[] background;
    private GameObject roleLobbyPanel;
    public GameObject gardenerRoleObject;
    public GameObject builderRoleObject;

    private Player player;
    private Player otherPlayer;
    
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
        roleLobbyPanel = transform.Find("RoleLobbyPanel").gameObject;
        roleLobbyPanel.SetActive(false);
        background[0].SetActive(false);
        background[1].SetActive(false);
    }

    public void InitPlayer(Player player)
    {
        this.player = player;
    }

    public void InitRoleSelector()
    {
        otherPlayer = MapManager.Instance.GetOtherPlayer(player);

        player.State = PlayerState.SelectingRole;

        player.Role.OnValueChanged += OnRoleChanged;
        otherPlayer.Role.OnValueChanged += OnRoleChanged;

        roleLobbyPanel.SetActive(true);
        background[0].SetActive(true);
        //background[1].SetActive(true);
        startGameObj.SetActive(NetworkManager.Singleton.IsHost);

        UpdateStartButton();
        UpdateInfoBoxText();
    }

    private void OnRoleChanged(PlayerRole previousValue, PlayerRole newValue)
    {
        UpdateStartButton();
        UpdateInfoBoxText();
    }

    private void UpdateStartButton()
    {
        bool bothReady = player.Role.Value != PlayerRole.None
            && otherPlayer.Role.Value != PlayerRole.None;

        lockObj.SetActive(!bothReady);
        
        canvasGroup.alpha = bothReady ? 1 : 0.4f;
        canvasGroup.interactable = bothReady;
        canvasGroup.blocksRaycasts = bothReady;
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
        string playerName = MapManager.Instance.GetPlayerById(playerId).name;
        
        bool isSelectionType = selectionRolType == SelectionRolType.Select;

        GameObject playerBox = roleLobbyPanel.transform.Find($"{playerRole}Box").gameObject;
        TextMeshProUGUI playerRoleText = playerBox.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        playerRoleText.text = isSelectionType ? playerName : "Sin escoger";
    }

    public void UpdateInfoBoxText()
    {
        TextMeshProUGUI infoboxText = roleLobbyPanel.transform.Find("InfoBox").GetComponentInChildren<TextMeshProUGUI>();

        infoboxText.text = (player.Role.Value, otherPlayer.Role.Value) switch
        {
            (PlayerRole.None, PlayerRole.None) => "Ningún jugador tiene rol",
            (PlayerRole.None, _) => $"{player.name} sin rol",
            (_, PlayerRole.None) => $"{otherPlayer.name} sin rol",
            _ => "Los 2 jugadores están listos.",
        };
    }

    public void StartGame()
    {
        player.StartGameServerRpc();
    }
    
    public void CloseLobby()
    {
        roleLobbyPanel.SetActive(false);
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
