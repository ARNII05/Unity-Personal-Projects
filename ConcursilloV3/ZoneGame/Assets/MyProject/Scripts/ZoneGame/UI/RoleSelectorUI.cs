using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoleSelectorUI : MonoBehaviour
{
    public static RoleSelectorUI Instance { get; private set; }

    public bool IsPlayerInitialized => player != null;
    
    private CanvasGroup canvasGroup;

    private GameObject background, gardenerRoleObject, builderRoleObject;
    private GameObject roleLobbyUI, startGameObj, player1Box, player2Box, lockObj, copyCodeBtn;
    private TextMeshProUGUI relayCodeText;

    private Player player;
    private Player otherPlayer;

    private bool gameStarted = false, roleSelectorInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        roleLobbyUI = transform.Find("RoleLobbyUI").gameObject;
        background = roleLobbyUI.transform.Find("Background").gameObject;
        gardenerRoleObject = roleLobbyUI.transform.Find("GardenerRole").gameObject;
        builderRoleObject = roleLobbyUI.transform.Find("BuilderRole").gameObject;
        copyCodeBtn = roleLobbyUI.transform.Find("CopyCodeBtn").gameObject;
        startGameObj = roleLobbyUI.transform.Find("StartGameButton").gameObject;
        canvasGroup = startGameObj.GetComponentInChildren<CanvasGroup>();
        relayCodeText = roleLobbyUI.transform.Find("RelayCodeText").GetComponent<TextMeshProUGUI>();
        player1Box = builderRoleObject.transform.Find("Player1Box").gameObject;
        player2Box = gardenerRoleObject.transform.Find("Player2Box").gameObject;
        lockObj = startGameObj.transform.Find("Lock").gameObject;

        roleLobbyUI.SetActive(false);
    }

    public void InitPlayer(Player player)
    {
        this.player = player;
    }

    public GameObject GetGardenerRoleObject()
    {
        return gardenerRoleObject;
    }

    public GameObject GetBuilderRoleObject()
    {
        return builderRoleObject;
    }

    public void InitRoleSelector()
    {
        if (player == null)
            return;

        otherPlayer = MapManager.Instance.GetOtherPlayer(player);

        if (!roleSelectorInitialized)
        {
            player.SetNewStateRpc(PlayerState.SelectingRole);

            player.Role.OnValueChanged += OnRoleChanged;

            roleLobbyUI.SetActive(true);
            background.SetActive(true);
            startGameObj.SetActive(NetworkManager.Singleton.IsServer);
            relayCodeText.gameObject.SetActive(NetworkManager.Singleton.IsServer);
            copyCodeBtn.SetActive(NetworkManager.Singleton.IsServer);

            if (NetworkManager.Singleton.IsServer)
            {
                relayCodeText.text = $"Código: {RelayManager.CurrentJoinCode}";
            }

            roleSelectorInitialized = true;
        }

        RefreshPlayers();
    }

    public void CopyRelayCode()
    {
        GUIUtility.systemCopyBuffer = RelayManager.CurrentJoinCode;
    }

    public void RefreshPlayers()
    {
        if (!roleSelectorInitialized)
            return;

        UpdateOtherPlayer();
        UpdateStartButton();
        UpdatePlayerBoxes();
        RefreshRoleSelections();
    }

    private void RefreshRoleSelections()
    {
        RefreshPlayerRole(MapManager.Instance.Player1);
        RefreshPlayerRole(MapManager.Instance.Player2);
    }

    private void RefreshPlayerRole(Player targetPlayer)
    {
        if (targetPlayer == null || targetPlayer == player)
            return;

        PlayerRole currentRole = targetPlayer.Role.Value;

        if (currentRole == PlayerRole.None)
            return;

        CommonActions(
            targetPlayer.OwnerClientId,
            currentRole,
            SelectionRolType.Select
        );

        player.RefreshDifferentPlayerRole(
            currentRole,
            targetPlayer.name
        );
    }

    private void UpdateOtherPlayer()
    {
        Player newOtherPlayer = MapManager.Instance.GetOtherPlayer(player);

        if (newOtherPlayer == null)
            ClearDisconnectedPlayerRole();

        if (otherPlayer == newOtherPlayer)
            return;

        if (otherPlayer != null)
            otherPlayer.Role.OnValueChanged -= OnRoleChanged;

        otherPlayer = newOtherPlayer;

        if (otherPlayer != null)
            otherPlayer.Role.OnValueChanged += OnRoleChanged;
    }

    private void ClearDisconnectedPlayerRole()
    {
        PlayerRole localRole = player.Role.Value;

        if (localRole != PlayerRole.Gardener)
            SetRoleTextBox(gardenerRoleObject, false);

        if (localRole != PlayerRole.Builder)
            SetRoleTextBox(builderRoleObject, false);

        GameObject disconnectedPlayerBox = MapManager.Instance.Player1 == null
            ? player1Box
            : player2Box;

        TextMeshProUGUI roleNameText = disconnectedPlayerBox.transform
            .GetChild(0)
            .Find("RolName")
            .GetComponent<TextMeshProUGUI>();

        roleNameText.text = "Sin escoger";
    }

    private void OnRoleChanged(PlayerRole previousValue, PlayerRole newValue)
    {
        UpdateStartButton();
        UpdatePlayerBoxes();
    }

    private void UpdateStartButton()
    {
        bool bothReady = otherPlayer != null &&
                         player.Role.Value != PlayerRole.None &&
                         otherPlayer.Role.Value != PlayerRole.None;

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
        Player playerWhoCalled = MapManager.Instance.GetPlayerById(playerId);
        
        bool isSelectionType = selectionRolType == SelectionRolType.Select;

        TextMeshProUGUI playerRolNameText;

        if (playerWhoCalled.OwnerClientId == MapManager.Instance.Player1.OwnerClientId)
            playerRolNameText = player1Box.transform.GetChild(0).Find("RolName").GetComponent<TextMeshProUGUI>();
        else
            playerRolNameText = player2Box.transform.GetChild(0).Find("RolName").GetComponent<TextMeshProUGUI>();

        playerRolNameText.text = isSelectionType ? (playerRole == PlayerRole.Builder ? "Constructor" : "Jardinero") : "Sin escoger";
    }

    public void UpdatePlayerBoxes()
    {
        TextMeshProUGUI player1NameText = player1Box.transform.GetChild(0)
            .Find("PlayerName").GetComponent<TextMeshProUGUI>();

        TextMeshProUGUI player2NameText = player2Box.transform.GetChild(0)
            .Find("PlayerName").GetComponent<TextMeshProUGUI>();

        Player player1 = MapManager.Instance.Player1;
        Player player2 = MapManager.Instance.Player2;

        player1NameText.text = player1 != null ? player1.name : "Esperando jugador...";
        player2NameText.text = player2 != null ? player2.name : "Esperando jugador...";
    }

    public void StartGame()
    {
        if (!gameStarted && NetworkManager.Singleton.IsServer)
            gameStarted = player.StartGame();
    }
    
    public void CloseLobby()
    {
        roleLobbyUI.SetActive(false);
        background.SetActive(false);
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
