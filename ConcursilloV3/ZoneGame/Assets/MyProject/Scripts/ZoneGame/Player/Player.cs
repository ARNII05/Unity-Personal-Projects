using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CraftingSystem))]
[RequireComponent(typeof(OtherPlayerVisual))]
[RequireComponent(typeof(PlayerNearbyObjects))]
[RequireComponent(typeof(ChestTransferSystem))]
[RequireComponent(typeof(PlayerAnimationController))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerRoleSelectionVisuals))]
public class Player : NetworkBehaviour
{
    private OtherPlayerVisual otherPlayerVisualController;
    private PlayerNearbyObjects nearbyObjects;
    private ChestTransferSystem chestTransferSystem;
    private PlayerAnimationController playerAnimationController;
    private PlayerInputHandler playerInputHandler;
    private PlayerRoleSelectionVisuals roleSelectionVisuals;

    public Chest nearbyChest => nearbyObjects.NearbyChest;
    public PlayerNearbyObjects NearbyObjects => nearbyObjects;
    public Vector2Int CurrentZoneMapPos;
    public Vector2Int initialPos;
    public GameObject currentZone;

    public Inventory inventory = new();
    public CraftingSystem craftingSystem;
    private InventoryUI inventoryUI;
    private Direction lastDirection;
    private ZoneGameControls controls;
    private WolfUI wolfUI;

    private bool gameplayInitialized;

    public HashSet<Vector2Int> discoveredZones = new();

    public NetworkVariable<PlayerState> State = new(
        PlayerState.Normal,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
     
    public NetworkVariable<Vector3> NetworkZoneEntryPosition = new(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsWalking = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public NetworkVariable<int> NetworkDirection = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public NetworkVariable<Vector2Int> NetworkMapPos = new(
        Vector2Int.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<PlayerRole> Role = new(
        PlayerRole.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        controls = new ZoneGameControls();
        craftingSystem = GetComponent<CraftingSystem>();
        otherPlayerVisualController = GetComponent<OtherPlayerVisual>();
        nearbyObjects = GetComponent<PlayerNearbyObjects>();
        chestTransferSystem = GetComponent<ChestTransferSystem>();
        playerAnimationController = GetComponent<PlayerAnimationController>();
        playerInputHandler = GetComponent<PlayerInputHandler>();
        playerInputHandler.Initialize(controls);
        roleSelectionVisuals = GetComponent<PlayerRoleSelectionVisuals>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        playerInputHandler.HandleInput();

        if (otherPlayerVisualController != null)
            otherPlayerVisualController.UpdateVisual();
    }

    public void OpenInventory()
    {
        inventoryUI.ToggleInventory();
    }

    public void CloseInventory()
    {
        inventoryUI.ToggleInventory();
    }

    public void OpenChest()
    {
        nearbyObjects.NearbyChest.OnOpen(this);
        UpdateOpenedChestServerRpc();
    }

    public void CloseChest()
    {
        nearbyObjects.NearbyChest.OnOpen(this);
    }

    public void CloseWolfUI()
    {
        wolfUI.CloseUI();
    }

    public void InteractWithRiver()
    {
        RiverInteractionServerRpc(NetworkMapPos.Value);
    }
    
    [Rpc(SendTo.Server)]
    public void SetNewStateRpc(PlayerState newState)
    {
        State.Value = newState;
    }

    [Rpc(SendTo.Server)]
    public void TradeItemRpc(ItemType item, ItemType itemToAdd, ItemType itemToRemove, int actualAmount, TradeMode tradeMode)
    {
        PurchaseResult result = PurchaseSystem.Instance.TradeItem(item, itemToAdd, itemToRemove, actualAmount, tradeMode, this);

        SetInventory();

        if (result == PurchaseResult.Success)
            PurchaseSystem.Instance.UpdateStock(tradeMode, item, actualAmount);

        TradeItemClientRpc(result, item, actualAmount, tradeMode);
    }

    [Rpc(SendTo.Everyone)]
    private void TradeItemClientRpc(PurchaseResult result, ItemType item, int actualAmount, TradeMode tradeMode)
    {
        bool purchaseIsASuccess = result == PurchaseResult.Success;

        if (purchaseIsASuccess && !IsServer)
            PurchaseSystem.Instance.UpdateStock(tradeMode, item, actualAmount);

        TradeUI.Instance.tradeObjects.UpdateUI(true);

        if (IsOwner)
        {
            TradeUI.Instance.tradeObjects.WriteOnAdviceText(result, true);
            //TradeUI.Instance.tradeWithCoins.DeselectItem();
        }
    }

    [Rpc(SendTo.Server)]
    private void RiverInteractionServerRpc(Vector2Int pos)
    {
        if (pos != NetworkMapPos.Value)
            return;

        if (MapManager.Instance.map[pos.y, pos.x].type != ZoneType.HorizontalRiver &&
            MapManager.Instance.map[pos.y, pos.x].type != ZoneType.VerticalRiver)
            return;

        if (!currentZone.TryGetComponent<River>(out var river))
            return;

        bool isBridged = river.Interact(this);

        if (!isBridged)
            return;

        if (!inventory.RemoveItem(ItemType.Log, 1))
            return;

        SetInventory();

        RiverInteractionClientRpc(pos);
    }

    [Rpc(SendTo.Everyone)]
    private void RiverInteractionClientRpc(Vector2Int position)
    {
        MapManager.Instance.map[position.y, position.x].riverBridged = true;

        Player localPlayer = MapManager.Instance.GetLocalPlayer();

        if (localPlayer == null)
            return;

        if (localPlayer.NetworkMapPos.Value != position)
            return;

        if (localPlayer.currentZone == null)
            return;

        if (!localPlayer.currentZone.TryGetComponent<River>(
                out var riverZone))
            return;

        riverZone.SwapGameObjectStatus();
    }

    public void ChangeZone()
    {
        if (State.Value == PlayerState.Transitioning)
            return;

        Direction direction = nearbyObjects.CurrentBorderZone.direction;
        Side side = nearbyObjects.CurrentBorderZone.side;

        MapManager.Instance.OnSwapingZone(
            this,
            direction,
            side
        );
    }

    public void InteractWithGrandma()
    {
        GrandmaUI.Instance.OpenPanel();
    }

    public void InteractWithTrade()
    {
        TradeUI.Instance.OpenUI();
    }

    [Rpc(SendTo.Server)]
    public void ShowEndGanePanelServerRpc()
    {
        State.Value = PlayerState.EndGame;

        Player otherPlayer = MapManager.Instance.GetOtherPlayer(this);

        otherPlayer.State.Value = PlayerState.EndGame;

        EndgameUI.Instance.ShowEndGamePanel();

        MapManager.Instance.wolf.SetReadyToMove(false);

        ShowEndGanePanelClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    private void ShowEndGanePanelClientRpc()
    {
        EndgameUI.Instance.ShowEndGamePanel();
    }

    [Rpc(SendTo.Server)]
    private void UpdateOpenedChestServerRpc()
    {
        UpdateOpenedChestClientRpc(NetworkMapPos.Value);
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateOpenedChestClientRpc(Vector2Int position)
    {
        MapManager.Instance.UpdateChestUINetworking(position);
    }

    [Rpc(SendTo.Server)]
    public void SendItemsToChestServerRpc(ItemType item, int amount)
    {
        if (!chestTransferSystem.TrySendItemsToChest(
                item, amount, out Vector2Int pos))
            return;

        SetInventory();

        SendItemsToChestClientRpc(item, amount, pos);
    }

    [Rpc(SendTo.Everyone)]
    private void SendItemsToChestClientRpc(
        ItemType item, int amount, Vector2Int pos)
    {
        if (!IsServer)
        {
            ZoneData zone = MapManager.Instance.map[pos.y, pos.x];
            zone.chestInventory.AddItem(item, amount);
        }

        Player localPlayer = MapManager.Instance.GetLocalPlayer();

        if (localPlayer != null && localPlayer.NetworkMapPos.Value == pos)
        {
            Chest chest = localPlayer.currentZone.GetComponentInChildren<Chest>();

            if (chest != null)
                chest.chestUI.chestInventoryUI.ChangeButtonStatus(true);
        }

        if (IsOwner)
            inventoryUI.ChangeButtonStatus(true);
    }

    [Rpc(SendTo.Server)]
    public void SendItemsToPlayerServerRpc(ItemType item, int amount)
    {
        if (!chestTransferSystem.TrySendItemsToPlayer(
                item, amount, out Vector2Int pos))
            return;

        SetInventory();

        SendItemsToPlayerClientRpc(item, amount, pos);
    }

    [Rpc(SendTo.Everyone)]
    private void SendItemsToPlayerClientRpc(ItemType item, int amount, Vector2Int pos)
    {
        if (!IsServer)
        {
            ZoneData zone = MapManager.Instance.map[pos.y, pos.x];
            zone.chestInventory.RemoveItem(item, amount);
        }

        Player localPlayer = MapManager.Instance.GetLocalPlayer();

        if (localPlayer != null && localPlayer.NetworkMapPos.Value == pos)
        {
            Chest chest = localPlayer.currentZone.GetComponentInChildren<Chest>();

            if (chest != null)
                chest.chestUI.chestInventoryUI.ChangeButtonStatus(true);
        }

        if (IsOwner)
            inventoryUI.ChangeButtonStatus(true);
    }

    [Rpc(SendTo.Server)]
    public void CraftCoreItemServerRpc()
    {
        if (!craftingSystem.TryCraftCoreItem())
            return;

        SetInventory();
    }

    [Rpc(SendTo.Server)]
    public void SetNetworkMapPosServerRpc(
        Vector2Int newPos,
        Direction direction,
        Vector3 entryPosition,
        bool changeLastDirection = true)
    {
        if (changeLastDirection)
            lastDirection = direction;

        NetworkZoneEntryPosition.Value = entryPosition;
        NetworkMapPos.Value = newPos;
    }

    [Rpc(SendTo.Server)]
    public void SendFirstRiverDirectionServerRpc(
        Vector2Int position,
        Direction direction)
    {
        ZoneData zoneData =
            MapManager.Instance.map[
                position.y,
                position.x
            ];

        if (zoneData.firstRiverDirection != Direction.None)
            return;

        zoneData.firstRiverDirection = direction;

        SendFirstRiverDirectionClientRpc(
            position,
            direction
        );
    }

    [Rpc(SendTo.Everyone)]
    private void SendFirstRiverDirectionClientRpc(
        Vector2Int position,
        Direction direction)
    {
        MapManager.Instance.map[
            position.y,
            position.x
        ].firstRiverDirection = direction;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkMapPos.OnValueChanged += OnNetworkMapPosChanged;
        IsWalking.OnValueChanged += OnWalkingChanged;
        NetworkDirection.OnValueChanged += OnDirectionChanged;

        SceneManager.sceneLoaded += OnSceneLoaded;

        TryInitializeGameplay();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "ZoneMap")
        {
            TryInitializeGameplay();
        }
    }

    private void TryInitializeGameplay()
    {
        if (gameplayInitialized || !IsSpawned)
            return;

        if (SceneManager.GetActiveScene().name != "ZoneMap")
            return;

        if (MapManager.Instance == null)
        {
            Debug.LogWarning(
                "Player: ZoneMap está activa, pero MapManager todavía no está disponible."
            );
            return;
        }

        gameplayInitialized = true;

        InitializeGameplay();
    }

    private void InitializeGameplay()
    {
        if (IsOwner)
        {
            controls.Gameplay.Enable();

            RoleSelectorUI.Instance.InitPlayer(this);

            GameObject inventoryPanel =
                GameObject.Find("InventoryPanel");

            inventoryUI =
                inventoryPanel.GetComponent<InventoryUI>();

            inventoryUI.SetPlayer(this);

            CraftingUI.Instance.SetPlayer(this);

            TradeUI.Instance.Init(this);

            wolfUI =
                GameObject.Find("WolfManager").GetComponent<WolfUI>();

            if (otherPlayerVisualController != null)
                otherPlayerVisualController.Initialize(this);
        }

        MapManager.Instance.RegisterPlayer(this, IsServer);
    }


    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        NetworkMapPos.OnValueChanged -= OnNetworkMapPosChanged;
        IsWalking.OnValueChanged -= OnWalkingChanged;
        NetworkDirection.OnValueChanged -= OnDirectionChanged;

        base.OnNetworkDespawn();
    }

    public override void OnDestroy()
    {
        controls?.Dispose();
        base.OnDestroy();
    }

    [Rpc(SendTo.Server)]
    public void DeselectRoleServerRpc(PlayerRole playerRole)
    {
        Role.Value = PlayerRole.None;

        SelectRoleClientRpc(
            playerRole,
            OwnerClientId,
            SelectionRolType.Deselect
        );
    }

    [Rpc(SendTo.Server)]
    public void SelectRoleServerRpc(PlayerRole playerRole)
    {
        if (Role.Value != PlayerRole.None)
            return;

        Role.Value = playerRole;

        SelectRoleClientRpc(
            playerRole,
            OwnerClientId,
            SelectionRolType.Select
        );
    }

    [Rpc(SendTo.Everyone)]
    public void SelectRoleClientRpc(
        PlayerRole playerRole,
        ulong playerId,
        SelectionRolType selectionRolType)
    {
        RoleSelectorUI.Instance.CommonActions(
            playerId,
            playerRole,
            selectionRolType
        );

        if (NetworkManager.Singleton.LocalClientId != playerId)
        {
            roleSelectionVisuals.UpdateDifferentPlayer(
                playerRole,
                selectionRolType,
                name
            );
        }
        else
        {
            roleSelectionVisuals.UpdateSamePlayer(
                playerRole,
                selectionRolType
            );
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ShowRecoverySystemPanelInfoClientRpc(ItemType[] items, int[] amounts)
    {
        RecoverySystemUI recoverySystemUI = ResourcesGenerator.Instance.recoverySystemUI;

        StartCoroutine(recoverySystemUI.ShowRecoverySystemPanelInfo(items, amounts));
    }

    public bool StartGame()
    {
        if (Role.Value == PlayerRole.None)
            return false;

        Player otherPlayer =
            MapManager.Instance.GetOtherPlayer(this);

        if (otherPlayer == null)
            return false;

        if (otherPlayer.Role.Value == PlayerRole.None)
            return false;

        GiveStartingItemsOnServer();
        otherPlayer.GiveStartingItemsOnServer();

        otherPlayer.GiveStartingItemClientRpc(otherPlayer.inventory.items.Keys.ToArray(), 
            otherPlayer.inventory.items.Values.ToArray(),
             otherPlayer.inventory.inventoryOrder.ToArray());

        StartGameClientRpc();
        otherPlayer.StartGameClientRpc();

        State.Value = PlayerState.Normal;
        otherPlayer.State.Value = PlayerState.Normal;

        return true;
    }

    private void GiveStartingItemsOnServer()
    {
        inventory.items.Clear();

        if (Role.Value == PlayerRole.Gardener)
        {
            inventory.AddItem(ItemType.Radar, 1);
            RadarUI.Instance.Init(this);
        }
        else if (Role.Value == PlayerRole.Builder)
        {
            inventory.AddItem(ItemType.Map, 1);
            MapUI.Instance.Init(this);
        }
        
        inventory.AddItem(ItemType.Meat, 1);

        /*
        inventory.AddItem(ItemType.Coin, 99);
        inventory.AddItem(ItemType.Branch, 99);
        inventory.AddItem(ItemType.Flower, 99);
        inventory.AddItem(ItemType.Log, 7);
        */
    }

    [Rpc(SendTo.Owner)]
    private void GiveStartingItemClientRpc(ItemType[] itemTypes, int[] amounts, ItemType[] order)
    {
        inventory.SetInventory(itemTypes, amounts, order);

        if (Role.Value == PlayerRole.Gardener)
        {
            RadarUI.Instance.Init(this);
        }
        else if (Role.Value == PlayerRole.Builder)
        {
            MapUI.Instance.Init(this);
        }
    }

    [Rpc(SendTo.Owner)]
    private void StartGameClientRpc()
    {
        RoleSelectorUI.Instance.CloseLobby();

        CraftingUI.Instance.Init();

        RabbitDialogue.Instance.Init(this);

        GrandmaUI.Instance.Init(this);

        MapManager.Instance.wolf.Init();
    }

    public void SetInventory()
    {
        if (!IsServer || OwnerClientId == NetworkManager.ServerClientId)
            return;

        ItemType[] items = inventory.items.Keys.ToArray();
        int[] amounts = inventory.items.Values.ToArray();
        ItemType[] order = inventory.inventoryOrder.ToArray();

        SetInventoryClientRpc(items, amounts, order, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SetInventoryClientRpc(
        ItemType[] items,
        int[] amounts,
        ItemType[] order,
        RpcParams rpcParams = default)
    {
        inventory.SetInventory(items, amounts, order);
    }

    public void InitRoleSelector()
    {
        InitRoleSelectorClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void InitRoleSelectorClientRpc()
    {
        StartCoroutine(InitRoleSelectorWhenReady());
    }

    private IEnumerator InitRoleSelectorWhenReady()
    {
        // Esperar a que la escena y la interfaz estén preparadas.
        while (RoleSelectorUI.Instance == null ||
               MapManager.Instance == null ||
               !RoleSelectorUI.Instance.IsPlayerInitialized ||
               !MapManager.Instance.ArePlayersRegistered ||
               MapManager.Instance.GetOtherPlayer(
                   MapManager.Instance.GetLocalPlayer()) == null)
        {
            yield return null;
        }

        RoleSelectorUI.Instance.InitRoleSelector();
    }

    private void OnWalkingChanged(bool oldValue, bool newValue)
    {
        playerAnimationController.UpdateAnimator();
    }

    private void OnDirectionChanged(int oldValue, int newValue)
    {
        playerAnimationController.UpdateAnimator();
    }

    private void OnNetworkMapPosChanged(
        Vector2Int oldPos,
        Vector2Int newPos)
    {
        MapManager mapManager = MapManager.Instance;

        if (mapManager == null)
            return;

        if (!IsOwner)
        {
            Player localPlayer = MapManager.Instance.GetLocalPlayer();

            if (localPlayer != null &&
                localPlayer.otherPlayerVisualController != null)
            {
                localPlayer.otherPlayerVisualController.HideTemporarily(0.15f);
            }
        }

        if (!MapManager.Instance.IsMapReady)
            return;

        MapManager.Instance.CreatePlayerZone(
            this,
            lastDirection
        );
    }

    [Rpc(SendTo.NotServer)]
    public void SendMapClientRpc(
        ZoneType[] zoneTypes,
        ItemType[] chestItemTypes,
        int[] chestAmounts,
        int[] chestItemCounts,
        Vector2Int[] positions)
    {
        StartCoroutine(ReceiveMapWhenReady(
            zoneTypes,
            chestItemTypes,
            chestAmounts,
            chestItemCounts,
            positions
        ));
    }

    private IEnumerator ReceiveMapWhenReady(
        ZoneType[] zoneTypes,
        ItemType[] chestItemTypes,
        int[] chestAmounts,
        int[] chestItemCounts,
        Vector2Int[] positions)
    {
        // No procesar los datos hasta que el gestor y ambos jugadores existan.
        while (MapManager.Instance == null ||
               !MapManager.Instance.ArePlayersRegistered)
        {
            yield return null;
        }

        MapManager.Instance.ReceiveMapFromServer(
            zoneTypes,
            chestItemTypes,
            chestAmounts,
            chestItemCounts,
            positions
        );
    }
}

public enum PlayerRole
{
    None,
    Gardener,
    Builder
}

public enum SelectionRolType
{
    Select,
    Deselect
}

public enum PlayerState
{
    Normal,
    SelectingRole,
    UsingRadar,
    UsingMap,
    RabbitDialogueInteraction,
    RabbitMapUI,
    TradeUI,
    GrandmaDialogueInteraction,
    CaughtByWolf,
    Inventory,
    Transitioning,
    Trading,
    EndGame
}