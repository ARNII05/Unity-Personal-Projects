using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using static Player;

[RequireComponent(typeof(CraftingSystem))]
public class Player : NetworkBehaviour
{
    public Vector2Int initialPos;
    public GameObject currentZone;
    private BorderZone currentBorderZone;
    private RiverInteraction currentRiverInteraction;
    private Chest nearbyChest;
    public Inventory inventory = new();
    public CraftingSystem craftingSystem;
    private InventoryUI inventoryUI;
    
    public PlayerState State { get; set; } = PlayerState.Normal;

    [SerializeField] private GameObject otherPlayerVisualPrefab;

    private GameObject otherPlayerVisualRoot;
    private GameObject otherPlayerVisual;

    private Animator otherPlayerAnimator;

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
        craftingSystem = GetComponent<CraftingSystem>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (currentRiverInteraction != null && Input.GetKeyDown(KeyCode.F))
        {
            RiverInteractionServerRpc();
        }
        else if (currentBorderZone != null && Input.GetKeyDown(KeyCode.F))
        {
            Direction direction = currentBorderZone.direction;
            currentBorderZone = null;
            MapManager.Instance.OnSwapingZone(this, direction);
        }
        else if (nearbyChest != null && Input.GetKeyDown(KeyCode.F))
        {
            nearbyChest.OnOpen(this);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryUI.ToggleInventory();
        }

        UpdateOtherPlayerVisual();
    }

    [ServerRpc]
    private void RiverInteractionServerRpc()
    {
        bool isBridged = currentRiverInteraction.SendInteractToRiver(this);
        
        if (!isBridged)
            return;
        
        RiverInteractionClientRpc(isBridged, NetworkMapPos.Value, OwnerClientId);
    }

    [ClientRpc]
    private void RiverInteractionClientRpc(
        bool isBridged,
        Vector2Int position,
        ulong playerId)
    {
        MapManager.Instance.map[
            position.y,
            position.x
        ].riverBridged = isBridged;

        if (!NetworkManager.Singleton.LocalClient.PlayerObject
                .TryGetComponent<Player>(out var localPlayer))
            return;

        if (localPlayer.NetworkMapPos.Value != position)
            return;

        if (localPlayer.currentZone == null)
            return;

        if (!localPlayer.currentZone.TryGetComponent<River>(
            out var riverZone))
            return;

        if (NetworkManager.Singleton.LocalClientId == playerId)
        {
            localPlayer.inventory.RemoveItem(ItemType.Log, 1);
        }

        riverZone.SwapGameObjectStatusNetworking();
    }

    [ServerRpc]
    public void SendItemToChestServerRpc(ItemType itemType, int amount)
    {
        UpdateChestInventoryClientRpc(
            itemType,
            amount,
            NetworkMapPos.Value
        );
    }

    [ClientRpc]
    private void UpdateChestInventoryClientRpc(ItemType itemType, int amount, Vector2Int position)
    {
        ZoneData zoneData = MapManager.Instance.map[position.y, position.x];

        zoneData.chestInventory.AddItem(itemType, amount);

        Chest chest = currentZone.GetComponentInChildren<Chest>();

        chest.chestUI.chestInventoryUI.ChangeButtonStatus(true);
    }

    [ServerRpc]
    public void RemoveItemToChestServerRpc(ItemType itemType, int amount)
    {
        RemoveItemFromChestClientRpc(
            itemType,
            amount,
            NetworkMapPos.Value
        );
    }

    [ClientRpc]
    private void RemoveItemFromChestClientRpc(ItemType itemType, int amount, Vector2Int position)
    {
        ZoneData zoneData = MapManager.Instance.map[position.y, position.x];

        zoneData.chestInventory.RemoveItem(itemType, amount);

        Chest chest = currentZone.GetComponentInChildren<Chest>();

        chest.chestUI.chestInventoryUI.ChangeButtonStatus(true);
    }

    [ServerRpc]
    public void SetNetworkMapPosServerRpc(Vector2Int newPos)
    {
        NetworkMapPos.Value = newPos;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkMapPos.OnValueChanged += OnNetworkMapPosChanged;
        IsWalking.OnValueChanged += OnWalkingChanged;
        NetworkDirection.OnValueChanged += OnDirectionChanged;

        if (IsOwner)
        {
            RoleSelectorUI.Instance.InitPlayer(this);
            GameObject inventoryPanel = GameObject.Find("InventoryPanel");
            inventoryUI = inventoryPanel.GetComponent<InventoryUI>();
            inventoryUI.SetPlayer(this);
            CraftingUI.Instance.SetPlayer(this);

            otherPlayerVisualRoot = new GameObject(
                "Other Player Visual Root"
            );

            otherPlayerVisualRoot.transform.SetPositionAndRotation(
                transform.position,
                transform.rotation
            );

            otherPlayerVisualRoot.transform.localScale =
                Vector3.one * 1.5f;

            otherPlayerVisual = Instantiate(
                otherPlayerVisualPrefab,
                otherPlayerVisualRoot.transform
            );

            otherPlayerAnimator =
                otherPlayerVisual.GetComponent<Animator>();

            otherPlayerVisualRoot.SetActive(false);
        }

        MapManager.Instance.RegisterPlayer(this, IsServer);
    }

    [ServerRpc]
    public void DeselectRoleServerRpc(PlayerRole playerRole)
    {
        Role.Value = PlayerRole.None;
        SelectRoleClientRpc(playerRole, OwnerClientId, SelectionRolType.Deselect);
    }

    [ServerRpc]
    public void SelectRoleServerRpc(PlayerRole playerRole)
    {
        if (Role.Value != PlayerRole.None)
            return;
        
        Role.Value = playerRole;
        SelectRoleClientRpc(playerRole, OwnerClientId, SelectionRolType.Select);
    }

    [ClientRpc]
    public void SelectRoleClientRpc(PlayerRole playerRole, ulong playerId, SelectionRolType selectionRolType)
    {
        RoleSelectorUI.Instance.CommonActions(playerId, playerRole, selectionRolType);
        
        if (NetworkManager.Singleton.LocalClientId != playerId)
            ActionsWithDifferentId(playerRole, selectionRolType);
        else
            ActionsWithSameId(playerRole, selectionRolType);
    }

    private void ActionsWithSameId(PlayerRole playerRole, SelectionRolType selectionRolType)
    {
        switch (selectionRolType)
        {
            case SelectionRolType.Select:
                if (playerRole == PlayerRole.Gardener)
                    RoleSelectorUI.Instance.SwapBox(RoleSelectorUI.Instance.gardenerRoleObject, true);
                else RoleSelectorUI.Instance.SwapBox(RoleSelectorUI.Instance.builderRoleObject, true);
                break;
            case SelectionRolType.Deselect:
                if (playerRole == PlayerRole.Gardener)
                    RoleSelectorUI.Instance.SwapBox(RoleSelectorUI.Instance.gardenerRoleObject, false);
                else RoleSelectorUI.Instance.SwapBox(RoleSelectorUI.Instance.builderRoleObject, false);
                break;
        }   
    }

    private void ActionsWithDifferentId(PlayerRole playerRole, SelectionRolType selectionRolType)
    {
        switch (selectionRolType)
        {
            case SelectionRolType.Select:
                if (playerRole == PlayerRole.Gardener)
                    RoleSelectorUI.Instance.SetRoleTextBox(
                        RoleSelectorUI.Instance.gardenerRoleObject, true,
                            $"{name} ha escogido el rol de Gardinero");
                else RoleSelectorUI.Instance.SetRoleTextBox(
                        RoleSelectorUI.Instance.builderRoleObject, true,
                            $"{name} ha escogido el rol de Constructor");
                break;
            case SelectionRolType.Deselect:
                if (playerRole == PlayerRole.Gardener)
                    RoleSelectorUI.Instance.SetRoleTextBox(
                        RoleSelectorUI.Instance.gardenerRoleObject, false);
                else RoleSelectorUI.Instance.SetRoleTextBox(
                        RoleSelectorUI.Instance.builderRoleObject, false);
                break;
        }
    }

    [ServerRpc]
    public void StartGameServerRpc()
    {
        if (Role.Value == PlayerRole.None)
            return;

        Player otherPlayer = MapManager.Instance.GetOtherPlayer(this);

        if (otherPlayer == null)
            return;

        if (otherPlayer.Role.Value == PlayerRole.None)
            return;

        GiveStartingItemClientRpc();
        otherPlayer.GiveStartingItemClientRpc();

        StartGameClientRpc();
        otherPlayer.StartGameClientRpc();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        if (!IsOwner)
            return;

        RoleSelectorUI.Instance.CloseLobby();
        CraftingUI.Instance.Init();
        State = PlayerState.Normal;
    }

    [ClientRpc]
    private void GiveStartingItemClientRpc()
    {
        if (!IsOwner)
            return;

        if (Role.Value == PlayerRole.Gardener)
            inventory.AddItem(ItemType.Radar, 1);

        else if (Role.Value == PlayerRole.Builder)
            inventory.AddItem(ItemType.Map, 1);
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
        UpdateAnimator();
    }

    private void OnDirectionChanged(int oldValue, int newValue)
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (!IsOwner)
            return;

        Player1Controller controller =
            GetComponent<Player1Controller>();

        if (controller == null || controller.anim == null)
            return;

        controller.anim.SetInteger(
            "Direction",
            NetworkDirection.Value
        );

        controller.anim.SetBool(
            "IsWalking",
            IsWalking.Value
        );
    }

    private void OnNetworkMapPosChanged(
        Vector2Int oldPos,
        Vector2Int newPos)
    {
        if (!MapManager.Instance.IsMapReady)
            return;

        MapManager.Instance.CreatePlayerZone(this);
    }

    private void UpdateOtherPlayerVisual()
    {
        if (!IsOwner)
            return;

        if (MapManager.Instance.Player1 == null ||
            MapManager.Instance.Player2 == null)
            return;

        if (currentZone == null)
            return;

        Player otherPlayer;

        if (this == MapManager.Instance.Player1)
            otherPlayer = MapManager.Instance.Player2;
        else
            otherPlayer = MapManager.Instance.Player1;

        if (otherPlayer.currentZone == null)
            return;

        bool sameZone =
            NetworkMapPos.Value ==
            otherPlayer.NetworkMapPos.Value;

        otherPlayerVisualRoot.SetActive(sameZone);

        if (!sameZone)
            return;

        otherPlayerVisualRoot.transform.position =
            currentZone.transform.position +
            (otherPlayer.transform.position -
             otherPlayer.currentZone.transform.position);

        otherPlayerAnimator.SetInteger(
            "Direction",
            otherPlayer.NetworkDirection.Value
        );

        otherPlayerAnimator.SetBool(
            "IsWalking",
            otherPlayer.IsWalking.Value
        );
    }

    [ClientRpc]
    public void SendMapClientRpc(
        NetworkZoneData[] networkMap,
        Vector2Int serverStartPos,
        Vector2Int serverGrandmaPos)
    {
        MapManager.Instance.ReceiveMapFromServer(
            networkMap,
            serverStartPos,
            serverGrandmaPos
        );
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Border":
                currentBorderZone =
                    other.GetComponent<BorderZone>();
                break;

            case "Chest":
                nearbyChest =
                    other.GetComponentInParent<Chest>();
                break;
            
            case "RiverInteractor":
                currentRiverInteraction = 
                    other.GetComponentInParent<RiverInteraction>();
                break;
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Border":
                currentBorderZone = null;
                break;

            case "Chest":
                nearbyChest = null;
                break;

            case "RiverInteractor":
                currentRiverInteraction = null;
                break;
        }
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
    Inventory,
    Interacting,
    Trading,
    Crafting
}