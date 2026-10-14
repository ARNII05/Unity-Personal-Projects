using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(CraftingSystem))]
public class Player : NetworkBehaviour
{
    private static readonly int DirectionHash = Animator.StringToHash("Direction");
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    public Vector2Int CurrentZoneMapPos;
    public Vector2Int initialPos;
    public GameObject currentZone;

    private BorderZone currentBorderZone;
    private RiverInteraction currentRiverInteraction;
    public Chest nearbyChest;
    private GameObject rabbit, nearbyGrandma;

    public Inventory inventory = new();
    public CraftingSystem craftingSystem;
    private InventoryUI inventoryUI;
    private Direction lastDirection;
    private ZoneGameControls controls;
    private WolfUI wolfUI;

    public PlayerState State { get; set; } = PlayerState.Normal;
    public HashSet<Vector2Int> discoveredZones = new();

    [SerializeField] private GameObject otherPlayerVisualPrefab;

    private GameObject otherPlayerVisualRoot;
    private GameObject otherPlayerVisual;

    private Animator otherPlayerAnimator; 

    private Coroutine showOtherPlayerCoroutine;

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
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        HandleInput();

        if (showOtherPlayerCoroutine == null)
            UpdateOtherPlayerVisual();
    }

    private void HandleInput()
    {
        switch (State)
        {
            case PlayerState.Normal:
                HandleNormalInput();
                break;

            case PlayerState.Trading:
                HandleTradingInput();
                break;

            case PlayerState.Inventory:
                HandleInventoryInput();
                break;

            case PlayerState.UsingMap:
                HandleMapInput();
                break;

            case PlayerState.UsingRadar:
                HandleRadarInput();
                break;

            case PlayerState.RabbitMapUI:
                HandleRabbitMapInput();
                break;

            case PlayerState.CaughtByWolf:
                HandleBeingCaughtByWolfInput();
                break;

            case PlayerState.SelectingRole:
            case PlayerState.RabbitDialogueInteraction:
            case PlayerState.GrandmaDialogueInteraction:
            case PlayerState.Transitioning:
            case PlayerState.Crafting:
            case PlayerState.EndGame:
                break;
        }
    }

    private void HandleNormalInput()
    {
        if (controls.Gameplay.InitialObj.WasPressedThisFrame())
            HandleTabInput();

        if (controls.Gameplay.Interact.WasPressedThisFrame())
            HandleInteractionInput();

        if (controls.Gameplay.Inventory.WasPressedThisFrame())
            OpenInventory();
    }

    private void HandleTradingInput()
    {
        if (controls.Gameplay.Interact.WasPressedThisFrame())
            CloseChest();
    }

    private void HandleInventoryInput()
    {
        if (controls.Gameplay.Inventory.WasPressedThisFrame())
            CloseInventory();
    }

    private void HandleMapInput()
    {
        if (controls.Gameplay.InitialObj.WasPressedThisFrame())
            MapUI.Instance.ToggleMap();
    }

    private void HandleRabbitMapInput()
    {
        if (controls.Gameplay.InitialObj.WasPressedThisFrame())
            RabbitDialogue.Instance.CloseMap();
    }

    private void HandleRadarInput()
    {
        if (controls.Gameplay.InitialObj.WasPressedThisFrame())
        {
            RadarUI.Instance.ToggleRadar();
        }
    }

    private void HandleBeingCaughtByWolfInput()
    {
        if (controls.Gameplay.Confirm.WasPressedThisFrame())
        {
            wolfUI.CloseUI(this);
        }
    }

    private void HandleTabInput()
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

    private void HandleInteractionInput()
    {
        if (currentRiverInteraction != null)
        {
            InteractWithRiver();
            return;
        }

        if (currentBorderZone != null)
        {
            ChangeZone();
            return;
        }

        if (nearbyChest != null)
        {
            OpenChest();
            return;
        }

        if (nearbyGrandma != null)
        {
            Debug.Log("Interacted with grandma");
            InteractWithGrandma();
            return;
        }

        if (rabbit != null)
        {
            RabbitDialogue.Instance.OpenDialogue();
        }
    }

    private void OpenInventory()
    {
        inventoryUI.ToggleInventory();
    }

    private void CloseInventory()
    {
        inventoryUI.ToggleInventory();
    }

    private void OpenChest()
    {
        nearbyChest.OnOpen(this);
        UpdateOpenedChestServerRpc();
    }

    private void CloseChest()
    {
        nearbyChest.OnOpen(this);
    }

    private void InteractWithRiver()
    {
        bool isBridged =
            currentRiverInteraction.SendInteractToRiver(this);

        if (!isBridged)
            return;

        inventory.RemoveItem(ItemType.Log, 1);

        RiverInteractionServerRpc(NetworkMapPos.Value);
    }

    private void ChangeZone()
    {
        Direction direction = currentBorderZone.direction;
        Side side = currentBorderZone.side;

        currentBorderZone = null;

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
    public void UpdateZoneBouquetsServerRpc(int amount)
    {
        ZoneData zoneData =
            MapManager.Instance.map[
                NetworkMapPos.Value.y,
                NetworkMapPos.Value.x
            ];

        zoneData.bouquetsInInventory += amount;

        UpdateZoneBouquetsClientRpc(amount);
    }

    [ClientRpc]
    public void UpdateZoneBouquetsClientRpc(int amount)
    {
        if (IsServer)
            return;

        ZoneData zoneData =
            MapManager.Instance.map[
                NetworkMapPos.Value.y,
                NetworkMapPos.Value.x
            ];

        zoneData.bouquetsInInventory += amount;
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
    private void RiverInteractionServerRpc(Vector2Int position)
    {
        MapManager.Instance.map[
            position.y,
            position.x
        ].riverBridged = true;

        RiverInteractionClientRpc(position);
    }

    [ClientRpc]
    private void RiverInteractionClientRpc(Vector2Int position)
    {
        MapManager.Instance.map[
            position.y,
            position.x
        ].riverBridged = true;

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

        riverZone.SwapGameObjectStatusNetworking();
    }

    [ServerRpc]
    private void UpdateOpenedChestServerRpc()
    {
        UpdateOpenedChestClientRpc(NetworkMapPos.Value);
    }

    [ClientRpc]
    private void UpdateOpenedChestClientRpc(Vector2Int position)
    {
        MapManager.Instance.UpdateChestUINetworking(
            OwnerClientId,
            position
        );
    }

    [ServerRpc]
    public void SendItemToChestServerRpc(
        ItemType itemType,
        int amount)
    {
        UpdateChestInventoryClientRpc(
            itemType,
            amount,
            NetworkMapPos.Value
        );
    }

    [ClientRpc]
    private void UpdateChestInventoryClientRpc(
        ItemType itemType,
        int amount,
        Vector2Int position)
    {
        ZoneData zoneData =
            MapManager.Instance.map[
                position.y,
                position.x
            ];

        zoneData.chestInventory.AddItem(
            itemType,
            amount
        );

        Chest chest =
            currentZone.GetComponentInChildren<Chest>();

        chest.chestUI.chestInventoryUI.ChangeButtonStatus(true);
    }

    [ServerRpc]
    public void RemoveItemToChestServerRpc(
        ItemType itemType,
        int amount)
    {
        RemoveItemFromChestClientRpc(
            itemType,
            amount,
            NetworkMapPos.Value
        );
    }

    [ClientRpc]
    private void RemoveItemFromChestClientRpc(
        ItemType itemType,
        int amount,
        Vector2Int position)
    {
        ZoneData zoneData =
            MapManager.Instance.map[
                position.y,
                position.x
            ];

        zoneData.chestInventory.RemoveItem(
            itemType,
            amount
        );

        Chest chest =
            currentZone.GetComponentInChildren<Chest>();

        chest.chestUI.chestInventoryUI.ChangeButtonStatus(true);
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

            otherPlayerVisualRoot =
                new GameObject("Other Player Visual Root");

            otherPlayerVisualRoot.transform.SetPositionAndRotation(
                transform.position,
                transform.rotation
            );

            otherPlayerVisualRoot.transform.localScale =
                Vector3.one * 1.5f;

            otherPlayerVisual =
                Instantiate(
                    otherPlayerVisualPrefab,
                    otherPlayerVisualRoot.transform
                );

            otherPlayerAnimator =
                otherPlayerVisual.GetComponent<Animator>();

            otherPlayerVisualRoot.SetActive(false);
        }

        MapManager.Instance.RegisterPlayer(
            this,
            IsServer
        );
    }

    public override void OnNetworkDespawn()
    {
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

        RabbitDialogue.Instance.Init(this);

        GrandmaUI.Instance.Init(this);

        MapManager.Instance.wolf.Init();

        State = PlayerState.Normal;
    }

    [ClientRpc]
    private void GiveStartingItemClientRpc()
    {
        if (!IsOwner)
            return;

        if (Role.Value == PlayerRole.Gardener)
        {
            inventory.AddItem(
                ItemType.Radar,
                1
            );
            RadarUI.Instance.Init(this);
        }
        else if (Role.Value == PlayerRole.Builder)
        {
            inventory.AddItem(
                ItemType.Map,
                1
            );
            MapUI.Instance.Init(this);
        }

        inventory.AddItem(ItemType.Meat, 1);
        inventory.AddItem(ItemType.Log, 7);
        inventory.AddItem(ItemType.Bouquet, 2);
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

    private void OnWalkingChanged(
        bool oldValue,
        bool newValue)
    {
        UpdateAnimator();
    }

    private void OnDirectionChanged(
        int oldValue,
        int newValue)
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
        if (!IsOwner)
        {
            Player localPlayer = MapManager.Instance.GetPlayerById(NetworkManager.Singleton.LocalClientId);

            if (localPlayer != null)
                localPlayer.HideOtherPlayerTemporarily(0.15f);
        }

        if (!MapManager.Instance.IsMapReady)
            return;

        MapManager.Instance.CreatePlayerZone(
            this,
            lastDirection
        );
    }

    private void UpdateOtherPlayerVisual()
    {
        if (!IsOwner)
            return;

        Player otherPlayer = MapManager.Instance.GetOtherPlayer(this);

        if (otherPlayer == null)
            return;

        if (currentZone == null)
        {
            otherPlayerVisualRoot.SetActive(false);
            return;
        }

        if (otherPlayer.currentZone == null)
        {
            otherPlayerVisualRoot.SetActive(false);
            return;
        }

        bool sameZone =
            NetworkMapPos.Value ==
            otherPlayer.NetworkMapPos.Value;

        if (!sameZone)
        {
            otherPlayerVisualRoot.SetActive(false);
            return;
        }

        Vector3 visualPosition =
            currentZone.transform.position +
            (otherPlayer.transform.position -
             otherPlayer.currentZone.transform.position);

        otherPlayerVisualRoot.transform.position =
            visualPosition;

        otherPlayerVisualRoot.SetActive(true);

        otherPlayerAnimator.SetInteger(
            DirectionHash,
            otherPlayer.NetworkDirection.Value
        );

        otherPlayerAnimator.SetBool(
            IsWalkingHash,
            otherPlayer.IsWalking.Value
        );
    }

    public void HideOtherPlayerTemporarily(float delay = 0.1f)
    {
        if (!IsOwner)
            return;

        if (showOtherPlayerCoroutine != null)
            StopCoroutine(showOtherPlayerCoroutine);

        otherPlayerVisualRoot.SetActive(false);

        showOtherPlayerCoroutine =
            StartCoroutine(ShowOtherPlayerAfterDelay(delay));
    }

    private IEnumerator ShowOtherPlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        UpdateOtherPlayerVisual();

        showOtherPlayerCoroutine = null;
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
            Debug.Log("No envio el mapa 2 veces. Soy el servidor");
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

            case "Grandma":

                nearbyGrandma = other.gameObject;

                break;

            case "Rabbit":

                rabbit = other.gameObject;
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

            case "Grandma":

                nearbyGrandma = null;

                break;

            case "Rabbit":

                rabbit = null;
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
    UsingRadar,
    UsingMap,
    RabbitDialogueInteraction,
    RabbitMapUI,
    GrandmaDialogueInteraction,
    CaughtByWolf,
    Inventory,
    Transitioning,
    Trading,
    Crafting,
    EndGame
}