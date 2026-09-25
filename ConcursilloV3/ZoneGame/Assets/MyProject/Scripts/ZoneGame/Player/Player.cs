using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : NetworkBehaviour
{
    public Vector2Int initialPos;
    public GameObject currentZone;
    private BorderZone currentBorderZone;
    private RiverInteraction currentRiverInteraction;
    private Chest nearbyChest;
    public Inventory inventory = new();

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
            OpenChestServerRpc();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            InventoryUI.Instance.ToggleInventory();
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
    private void OpenChestServerRpc()
    {
        Vector2Int position = NetworkMapPos.Value;

        nearbyChest.OnOpen(this);
        nearbyChest = null;

        MapManager.Instance.OnChestOpen(position);

        OpenChestClientRpc(position);

        SyncInventory();
    }

    [ClientRpc]
    private void OpenChestClientRpc(Vector2Int position)
    {
        MapManager.Instance.OnChestOpenedNetworked(position);

        Player actualPlayer = IsHost
            ? MapManager.Instance.Player1
            : MapManager.Instance.Player2;

        MapManager.Instance.DisableChestAtPosition(
            position,
            actualPlayer
        );
    }

    private void SyncInventory()
    {
        ItemType[] itemTypes = new ItemType[inventory.items.Count];
        int[] amounts = new int[inventory.items.Count];

        int i = 0;

        foreach (var item in inventory.items)
        {
            itemTypes[i] = item.Key;
            amounts[i] = item.Value;
            i++;
        }

        ClientRpcParams rpcParams = new()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { OwnerClientId }
            }
        };

        SyncInventoryClientRpc(itemTypes, amounts, rpcParams);
    }

    [ClientRpc]
    private void SyncInventoryClientRpc(
        ItemType[] itemTypes,
        int[] amounts,
        ClientRpcParams clientRpcParams = default)
    {
        inventory.SetItems(itemTypes, amounts);
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
            InventoryUI.Instance.SetInventory(inventory);

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

        MapManager.Instance.UpdateActualZone(this);
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