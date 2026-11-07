
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GrandmaUI : NetworkBehaviour
{
    public static GrandmaUI Instance { get; private set; }

    [SerializeField] private GameObject grandmaPanel;

    private TextMeshProUGUI requirement1;
    private TextMeshProUGUI requirement2;

    private Button acceptBtn;
    private GameObject denyTextObj;

    private Player player;
    private Player otherPlayer;

    private int bouquetCount = 0;

    private bool initialized = false;
    private bool uiReady = false;

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
        grandmaPanel.SetActive(false);

        acceptBtn = grandmaPanel.transform.Find("Note")
            .Find("AcceptBtn").GetComponent<Button>();

        denyTextObj = acceptBtn.transform.GetChild(0).gameObject;
        denyTextObj.SetActive(false);

        requirement1 = grandmaPanel.transform.Find("Note")
            .Find("Requirement1").GetComponentInChildren<TextMeshProUGUI>();

        requirement2 = grandmaPanel.transform.Find("Note")
            .Find("Requirement2").GetComponentInChildren<TextMeshProUGUI>();

        uiReady = true;

        UpdateGrandmaPanel();
    }

    public void Init(Player player)
    {
        if (initialized)
            return;

        this.player = player;
        otherPlayer = MapManager.Instance.GetOtherPlayer(player);

        initialized = true;

        if (IsServer)
        {
            player.inventory.OnInventoryChanged += UpdateBouquetCount;
            otherPlayer.inventory.OnInventoryChanged += UpdateBouquetCount;

            player.NetworkMapPos.OnValueChanged += OnPlayerPositionChanged;
            otherPlayer.NetworkMapPos.OnValueChanged += OnPlayerPositionChanged;

            UpdateBouquetCount();
        }
    }

    private void OnPlayerPositionChanged(Vector2Int oldPos, Vector2Int newPos)
    {
        if (!IsServer)
            return;

        UpdateBouquetCount();
    }

    private void UpdateBouquetCount()
    {
        if (!IsServer || !initialized)
            return;

        bouquetCount =
            player.inventory.GetAmount(ItemType.Bouquet) +
            otherPlayer.inventory.GetAmount(ItemType.Bouquet);

        UpdateBouquetCountClientRpc(bouquetCount);
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateBouquetCountClientRpc(int newBouquetCount)
    {
        bouquetCount = newBouquetCount;

        UpdateGrandmaPanel();
    }

    private void UpdateGrandmaPanel()
    {
        if (!uiReady || !initialized)
            return;

        int bouquetsToWin = CraftingSystem.BouquetsToWin;

        bool canWin = CanWin(bouquetCount, bouquetsToWin);

        requirement1.text = $" ->    {bouquetCount} / {bouquetsToWin}";

        requirement2.text =
            player.NetworkMapPos.Value != otherPlayer.NetworkMapPos.Value
            ? $" ->    {player.name}"
            : $" ->    {player.name} / {otherPlayer.name}";

        denyTextObj.SetActive(!canWin);
        acceptBtn.interactable = canWin;
    }

    public void OpenPanel()
    {
        player.SetNewStateRpc(PlayerState.GrandmaDialogueInteraction);

        grandmaPanel.SetActive(true);

        UpdateGrandmaPanel();
    }

    public void EndGame()
    {
        ShowEndGamePanelServerRpc();
    }

    public void CloseUI()
    {
        player.SetNewStateRpc(PlayerState.Normal);

        grandmaPanel.SetActive(false);
    }

    private bool CanWin(int totalBouquets, int bouquetsToWin)
    {
        return totalBouquets >= bouquetsToWin
            && player.NetworkMapPos.Value == otherPlayer.NetworkMapPos.Value;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ShowEndGamePanelServerRpc(RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (senderId != player.OwnerClientId &&
            senderId != otherPlayer.OwnerClientId)
            return;

        int totalBouquets =
            player.inventory.GetAmount(ItemType.Bouquet) +
            otherPlayer.inventory.GetAmount(ItemType.Bouquet);

        if (!CanWin(totalBouquets, CraftingSystem.BouquetsToWin))
            return;

        if (player.State.Value == PlayerState.EndGame ||
            otherPlayer.State.Value == PlayerState.EndGame)
            return;

        player.State.Value = PlayerState.EndGame;
        otherPlayer.State.Value = PlayerState.EndGame;

        MapManager.Instance.wolf.SetReadyToMove(false);

        ShowEndGamePanelClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ShowEndGamePanelClientRpc()
    {
        EndgameUI.Instance.ShowEndGamePanel();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log(
            $"GrandmaUI | LocalClientId: {NetworkManager.LocalClientId} | " +
            $"OwnerClientId: {OwnerClientId} | " +
            $"IsOwner: {IsOwner} | IsServer: {IsServer}"
        );
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && initialized)
        {
            player.inventory.OnInventoryChanged -= UpdateBouquetCount;
            otherPlayer.inventory.OnInventoryChanged -= UpdateBouquetCount;

            player.NetworkMapPos.OnValueChanged -= OnPlayerPositionChanged;
            otherPlayer.NetworkMapPos.OnValueChanged -= OnPlayerPositionChanged;
        }

        if (Instance == this)
            Instance = null;

        base.OnNetworkDespawn();
    }
}
