using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CraftingSystem))]
[RequireComponent(typeof(OtherPlayerVisual))]
[RequireComponent(typeof(PlayerNearbyObjects))]
[RequireComponent(typeof(ChestTransferSystem))]
[RequireComponent(typeof(PlayerAnimationController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class Player : NetworkBehaviour
{
    private OtherPlayerVisual otherPlayerVisualController;
    private PlayerNearbyObjects nearbyObjects;
    private ChestTransferSystem chestTransferSystem;
    private PlayerAnimationController playerAnimationController;
    private PlayerInputHandler playerInputHandler;

    public Chest nearbyChest => nearbyObjects.NearbyChest;
    public Vector2Int CurrentZoneMapPos;
    public Vector2Int initialPos;
    public GameObject currentZone;

    public Inventory inventory = new();
    public CraftingSystem craftingSystem;
    private InventoryUI inventoryUI;
    private Direction lastDirection;
    private ZoneGameControls controls;
    private WolfUI wolfUI;

    public PlayerState State { get; set; } = PlayerState.Normal;
    public HashSet<Vector2Int> discoveredZones = new();

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
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        playerInputHandler.HandleInput();

        if (otherPlayerVisualController != null)
            otherPlayerVisualController.UpdateVisual();
    }

    public void HandleTabInput()
    {
        if (inventory.GetItemAmount(ItemType.Radar) > 0)
        {
            RadarUI.Instance.ToggleRadar();
            return;
        }

        if (inventory.GetItemAmount(ItemType.Map) > 0)
        {
            MapUI.Instance.ToggleMap();
        }
    }

    public void HandleInteractionInput()
    {
        if (nearbyObjects.CurrentRiverInteraction != null)
        {
            RiverInteractionServerRpc(NetworkMapPos.Value);
            return;
        }

        if (nearbyObjects.CurrentBorderZone != null)
        {
            ChangeZone();
            return;
        }

        if (nearbyObjects.NearbyChest != null)
        {
            OpenChest();
            return;
        }

        if (nearbyObjects.NearbyGrandma != null)
        {
            InteractWithGrandma();
            return;
        }

        if (nearbyObjects.Rabbit != null)
        {
            RabbitDialogue.Instance.OpenDialogue();
        }
    }

    public void OpenInventory()
    {
        inventoryUI.ToggleInventory();
    }

    public void CloseInventory()
    {
        inventoryUI.ToggleInventory();
    }

    private void OpenChest()
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

    [ServerRpc]
    private void RiverInteractionServerRpc(Vector2Int pos)
    {
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

    [ClientRpc]
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

    private void ChangeZone()
    {
        Direction direction = nearbyObjects.CurrentBorderZone.direction;
        Side side = nearbyObjects.CurrentBorderZone.side;

        MapManager.Instance.OnSwapingZone(
            this,
            direction,
            side
        );
    }

    private void InteractWithGrandma()
    {
        GrandmaUI.Instance.OpenPanel();
    }

    [ServerRpc]
    public void ShowEndGanePanelServerRpc()
    {
        State = PlayerState.EndGame;

        EndgameUI.Instance.ShowEndGamePanel();

        ShowEndGanePanelClientRpc();
    }

    [ClientRpc]
    private void ShowEndGanePanelClientRpc()
    {
        if (IsServer)
            return;

        State = PlayerState.EndGame;

        EndgameUI.Instance.ShowEndGamePanel();
    }

    [ServerRpc]
    private void UpdateOpenedChestServerRpc()
    {
        UpdateOpenedChestClientRpc(NetworkMapPos.Value);
    }

    [ClientRpc]
    private void UpdateOpenedChestClientRpc(Vector2Int position)
    {
        MapManager.Instance.UpdateChestUINetworking(position);
    }

    [ServerRpc]
    public void SendItemsToChestServerRpc(ItemType item, int amount)
    {
        if (!chestTransferSystem.TrySendItemsToChest(
                item, amount, out Vector2Int pos))
            return;

        SetInventory();

        SendItemsToChestClientRpc(item, amount, pos);
    }

    [ClientRpc]
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

    [ServerRpc]
    public void SendItemsToPlayerServerRpc(ItemType item, int amount)
    {
        if (!chestTransferSystem.TrySendItemsToPlayer(
                item, amount, out Vector2Int pos))
            return;

        SetInventory();

        SendItemsToPlayerClientRpc(item, amount, pos);
    }

    [ClientRpc]
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

    [ServerRpc]
    public void CraftCoreItemServerRpc()
    {
        if (!craftingSystem.TryCraftCoreItem())
            return;

        SetInventory();
    }

    [ServerRpc]
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

    [ServerRpc]
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

    [ClientRpc]
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

            wolfUI = GameObject.Find("WolfManager").GetComponent<WolfUI>();

            if (otherPlayerVisualController != null)
                otherPlayerVisualController.Initialize(this);
        }

        MapManager.Instance.RegisterPlayer(
            this,
            IsServer
        );
    }

    public override void OnNetworkDespawn()
    {
        NetworkMapPos.OnValueChanged -= OnNetworkMapPosChanged;
        IsWalking.OnValueChanged -= OnWalkingChanged;
        NetworkDirection.OnValueChanged -= OnDirectionChanged;

        controls.Gameplay.Disable();

        base.OnNetworkDespawn();
    }

    public override void OnDestroy()
    {
        controls?.Dispose();
        base.OnDestroy();
    }

    [ServerRpc]
    public void DeselectRoleServerRpc(PlayerRole playerRole)
    {
        Role.Value = PlayerRole.None;

        SelectRoleClientRpc(
            playerRole,
            OwnerClientId,
            SelectionRolType.Deselect
        );
    }

    [ServerRpc]
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

    [ClientRpc]
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
            ActionsWithDifferentId(
                playerRole,
                selectionRolType
            );
        else
            ActionsWithSameId(
                playerRole,
                selectionRolType
            );
    }

    private void ActionsWithSameId(
        PlayerRole playerRole,
        SelectionRolType selectionRolType)
    {
        switch (selectionRolType)
        {
            case SelectionRolType.Select:

                if (playerRole == PlayerRole.Gardener)
                {
                    RoleSelectorUI.Instance.SwapBox(
                        RoleSelectorUI.Instance.gardenerRoleObject,
                        true
                    );
                }
                else
                {
                    RoleSelectorUI.Instance.SwapBox(
                        RoleSelectorUI.Instance.builderRoleObject,
                        true
                    );
                }

                break;

            case SelectionRolType.Deselect:

                if (playerRole == PlayerRole.Gardener)
                {
                    RoleSelectorUI.Instance.SwapBox(
                        RoleSelectorUI.Instance.gardenerRoleObject,
                        false
                    );
                }
                else
                {
                    RoleSelectorUI.Instance.SwapBox(
                        RoleSelectorUI.Instance.builderRoleObject,
                        false
                    );
                }

                break;
        }
    }

    private void ActionsWithDifferentId(
        PlayerRole playerRole,
        SelectionRolType selectionRolType)
    {
        switch (selectionRolType)
        {
            case SelectionRolType.Select:

                if (playerRole == PlayerRole.Gardener)
                {
                    RoleSelectorUI.Instance.SetRoleTextBox(
                        RoleSelectorUI.Instance.gardenerRoleObject,
                        true,
                        $"{name} ha escogido el rol de Gardinero"
                    );
                }
                else
                {
                    RoleSelectorUI.Instance.SetRoleTextBox(
                        RoleSelectorUI.Instance.builderRoleObject,
                        true,
                        $"{name} ha escogido el rol de Constructor"
                    );
                }

                break;

            case SelectionRolType.Deselect:

                if (playerRole == PlayerRole.Gardener)
                {
                    RoleSelectorUI.Instance.SetRoleTextBox(
                        RoleSelectorUI.Instance.gardenerRoleObject,
                        false
                    );
                }
                else
                {
                    RoleSelectorUI.Instance.SetRoleTextBox(
                        RoleSelectorUI.Instance.builderRoleObject,
                        false
                    );
                }

                break;
        }
    }

    [ServerRpc]
    public void StartGameServerRpc()
    {
        if (Role.Value == PlayerRole.None)
            return;

        Player otherPlayer =
            MapManager.Instance.GetOtherPlayer(this);

        if (otherPlayer == null)
            return;

        if (otherPlayer.Role.Value == PlayerRole.None)
            return;

        GiveStartingItemsOnServer();
        otherPlayer.GiveStartingItemsOnServer();

        GiveStartingItemClientRpc(inventory.items.Keys.ToArray(), 
            inventory.items.Values.ToArray());
        otherPlayer.GiveStartingItemClientRpc(otherPlayer.inventory.items.Keys.ToArray(), 
            otherPlayer.inventory.items.Values.ToArray());

        StartGameClientRpc();
        otherPlayer.StartGameClientRpc();
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
        inventory.AddItem(ItemType.Bouquet, 7);
        inventory.AddItem(ItemType.Log, 7);
        inventory.AddItem(ItemType.Coin, 7);
    }

    [ClientRpc]
    private void GiveStartingItemClientRpc(ItemType[] itemTypes, int[] amounts)
    {
        if (!IsOwner || IsServer)
            return;
        
        inventory.SetInventory(itemTypes, amounts);

        if (Role.Value == PlayerRole.Gardener)
        {
            RadarUI.Instance.Init(this);
        }
        else if (Role.Value == PlayerRole.Builder)
        {
            MapUI.Instance.Init(this);
        }
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        if (!IsOwner)
            return;

        RoleSelectorUI.Instance.CloseLobby();

        CraftingUI.Instance.Init();

        RabbitDialogue.Instance.Init(this);

        GrandmaUI.Instance.Init(this);

        MapManager.Instance.wolf.Init();

        State = PlayerState.Normal;
    }

    public void SetInventory()
    {
        if (!IsServer)
            return;

        ItemType[] items = inventory.items.Keys.ToArray();
        int[] amounts = inventory.items.Values.ToArray();

        ClientRpcParams rpcParams = new()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { OwnerClientId }
            }
        };

        SetInventoryClientRpc(items, amounts, rpcParams);
    }

    [ClientRpc]
    private void SetInventoryClientRpc(
        ItemType[] items,
        int[] amounts,
        ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;

        inventory.SetInventory(items, amounts);
    }

    public void InitRoleSelector()
    {
        InitRoleSelectorClientRpc();
    }

    [ClientRpc]
    private void InitRoleSelectorClientRpc()
    {
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

    [ClientRpc]
    public void SendMapClientRpc(
        ZoneType[] zoneTypes,
        ItemType[] chestItemTypes,
        int[] chestAmounts,
        int[] chestItemCounts,
        Vector2Int[] positions)
    {
        if (IsServer)
        {
            return;
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
    Crafting,
    EndGame
}