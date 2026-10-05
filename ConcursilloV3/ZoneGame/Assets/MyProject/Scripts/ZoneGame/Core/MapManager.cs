using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class MapManager : MonoBehaviour
{
    public static MapManager instance { get; private set; }
    public bool IsMapReady { get; private set; } = false;

    public Vector2Int grandmaPos;

    public Vector2Int startPos;

    public Vector2Int wolfDenPos;

    public const int mapWidth = 7;
    public const int mapHeight = 7;
    private const int MinAccessibleTilesBeforeRiver = 10;

    public ZoneData[,] map = new ZoneData[mapHeight, mapWidth];

    [SerializeField] private Vector3 player1RealPos;
    [SerializeField] private Vector3 player2RealPos;

    private const string ZonePrefabPath = "Prefabs/Zones/";

    private Player player1;
    private Player player2;

    public Player Player1 => player1;
    public Player Player2 => player2;

    public static MapManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MapManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        CleanMap();
    }

    public void RegisterPlayer(Player player, bool isServer)
    {
        if (player1 == null)
        {
            player1 = player;
            player1.name = "Player 1";
        }
        else if (player2 == null)
        {
            player2 = player;
            player2.name = "Player 2";
        }

        if (player1 != null &&
            player2 != null &&
            isServer)
        {
            InitRoleSelector();
            MakeRandomMap();
        }
    }

    private void CleanMap()
    {
        Zone[] zones = FindObjectsByType<Zone>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var zone in zones)
        {
            Destroy(zone.gameObject);
        }
    }
    
    public Player GetOtherPlayer(Player player)
    {
        if (player == player1)
            return player2;

        if (player == player2)
            return player1;

        return null;
    }

    public Player GetPlayerById(ulong playerId)
    {
        if (playerId == player1.OwnerClientId)
            return player1;

        if (playerId == player2.OwnerClientId)
            return player2;

        return null;
    }

    private void InitRoleSelector()
    {
        StartCoroutine(InitRoleSelectorCoroutine());
    }

    private IEnumerator InitRoleSelectorCoroutine()
    {
        yield return null;

        player1.InitRoleSelector();
    }

    public void MakeRandomMap()
    {
        const int maxAttempts = 100;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            map = new ZoneData[mapHeight, mapWidth];

            GenerateStartPositions();

            InitPlayersInfo(startPos);

            map[startPos.y, startPos.x] = new MomHouse();
            map[grandmaPos.y, grandmaPos.x] = new GrandmaHouse();

            ImplementRiver(startPos);

            if (HasEnoughAccessibleTilesBeforeRiver())
            {
                FillOtherZones();

                player1.NetworkMapPos.Value = startPos;
                player2.NetworkMapPos.Value = startPos;

                PrintMap();
                CreateInitialZones();
                ResourcesGenerator.Instance.Init();
                IsMapReady = true;
                StartCoroutine(SendMapNextFrame());

                return;
            }
        }

        Debug.LogError(
            "No se pudo generar un mapa valido despues de 100 intentos."
        );
    }

    private void GenerateStartPositions()
    {
        startPos = new Vector2Int(
            Random.Range(0, mapWidth),
            Random.Range(0, mapHeight)
        );

        grandmaPos = GrandmaHousePos(startPos);
    }

    private IEnumerator SendMapNextFrame()
    {
        yield return null;

        SendMapToClient();
    }

    private void SendMapToClient()
    {
        ZoneType[] zoneTypes =
            new ZoneType[mapWidth * mapHeight];

        List<ItemType> chestItemTypesList =
            new();

        List<int> chestAmountsList =
            new();

        int[] chestItemCounts =
            new int[mapWidth * mapHeight];

        int index = 0;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                ZoneData zone = map[y, x];

                zoneTypes[index] = zone.type;

                int itemCount = zone.chestInventory.items.Count;

                chestItemCounts[index] = itemCount;

                foreach (var item in
                         zone.chestInventory.items)
                {
                    chestItemTypesList.Add(item.Key);
                    chestAmountsList.Add(item.Value);
                }

                index++;
            }
        }

        player1.SendMapClientRpc(
            zoneTypes,
            chestItemTypesList.ToArray(),
            chestAmountsList.ToArray(),
            chestItemCounts,
            new[] { startPos, grandmaPos, wolfDenPos }
        );
    }

    public void ReceiveMapFromServer(
        ZoneType[] zoneTypes,
        ItemType[] chestItemTypes,
        int[] chestAmounts,
        int[] chestItemCounts,
        Vector2Int[] positions)
    {
        startPos = positions[0];
        grandmaPos = positions[1];
        wolfDenPos = positions[2];

        int index = 0;
        int chestItemIndex = 0;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                ZoneData zone =
                    ZoneVault.GenerateZone(zoneTypes[index]);

                if (zone == null)
                {
                    index++;
                    continue;
                }

                zone.chestInventory = new Inventory();

                int itemCount = chestItemCounts[index];

                for (int i = 0; i < itemCount; i++)
                {
                    ItemType item =
                        chestItemTypes[chestItemIndex];

                    int amount =
                        chestAmounts[chestItemIndex];

                    zone.chestInventory.AddItem(
                        item,
                        amount
                    );

                    chestItemIndex++;
                }

                map[y, x] = zone;

                index++;
            }
        }

        InitPlayersInfo(startPos, false);

        CreateInitialZones();

        IsMapReady = true;

        PrintMap();
    }

    private void InitPlayersInfo(Vector2Int startPos, bool IsServer = true)
    {
        if (player1 == null ||
            player2 == null)
        {
            return;
        }

        player1.initialPos = startPos;
        player2.initialPos = startPos;

        player1.transform.position =
            player1RealPos;

        player2.transform.position =
            player2RealPos;

        if (!IsServer)
            return;

        player1.NetworkZoneEntryPosition.Value =
            player1.transform.position;

        player2.NetworkZoneEntryPosition.Value =
            player2.transform.position;
    }

    private void CreateInitialZones()
    {
        if (player1 == null ||
            player2 == null)
        {
            return;
        }

        CreatePlayerZone(player1);

        CreatePlayerZone(player2);
    }

    public void CreatePlayerZone(Player player, Direction direction = Direction.South)
    {
        if (player.currentZone != null)
        {
            Destroy(player.currentZone);
        }

        Vector2Int position =
            player.NetworkMapPos.Value;

        ZoneData zoneData =
            map[position.y, position.x];

        GameObject prefab =
            LoadZone(zoneData.type);

        if (prefab == null)
        {
            return;
        }

        Vector3 zonePosition =
            player == player1
                ? zoneData.player1ZonePos
                : zoneData.player2ZonePos;

        GameObject zone = Instantiate(
            prefab,
            zonePosition,
            Quaternion.identity
        );

        player.currentZone = zone;
        player.CurrentZoneMapPos = position;

        Zone currentZone = zone.GetComponent<Zone>();

        Chest[] chests = zone.GetComponentsInChildren<Chest>(true);

        foreach (Chest chest in chests)
        {
            chest.Init(GameObject.Find("ChestUI").GetComponent<ChestUI>());
            chest.inventory = zoneData.chestInventory;
        }

        if (player.IsOwner)
        {
            UIManager.Instance.UpdatePlayerUI(position, zoneData.GetName());
            chests[0].chestUI.SetInventory(zoneData.chestInventory);
        }

        if (zoneData.chestOpened)
        {
            foreach (Chest chest in chests)
            {
                chest.gameObject.GetComponent<SpriteRenderer>().sprite
                = Resources.Load<Sprite>("Prefabs/Objects/OpenedBox");
            }
        }
           
        currentZone.Setup(
            position,
            map
        );

        Debug.Log($"Player {player.name} is owner: {player.IsOwner}");

        if (NetworkManager.Singleton.IsServer && (direction == Direction.North ||
             direction == Direction.South) &&
            zoneData.firstRiverDirection == Direction.None 
            && zoneData.type == ZoneType.HorizontalRiver)
        {
            player1.SendFirstRiverDirectionServerRpc(position, direction == Direction.North ? Direction.South : Direction.North);
        }

        if (zoneData.type == ZoneType.HorizontalRiver ||
            zoneData.type == ZoneType.VerticalRiver)
        {
            River actualRiver = zone.GetComponent<River>();
            actualRiver.Init(position);
        }
    }

    private GameObject LoadZone(ZoneType zoneType)
    {
        GameObject prefab =
            Resources.Load<GameObject>(
                ZonePrefabPath + zoneType.ToString()
            );

        if (prefab != null) return prefab;

        return Resources.Load<GameObject>(
                ZonePrefabPath + "MomHouse");
    }

    private Vector2Int GrandmaHousePos(Vector2Int startPos)
    {
        const int minAxisDistance = 2;

        List<Vector2Int> possiblePositions = new();

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2Int pos = new(x, y);

                if (pos == startPos)
                    continue;

                int dx = Mathf.Abs(startPos.x - x);
                int dy = Mathf.Abs(startPos.y - y);

                if (dx >= minAxisDistance &&
                    dy >= minAxisDistance)
                {
                    possiblePositions.Add(pos);
                }
            }
        }

        return possiblePositions[
            Random.Range(0, possiblePositions.Count)
        ];
    }

    private void PutWolfDenPos(Vector2Int startPos, Vector2Int grandmaPos)
    {
        const int minAxisDistance = 2;

        List<Vector2Int> possiblePositions = new();

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2Int pos = new(x, y);

                if (pos == startPos || pos == grandmaPos)
                    continue;

                if (map[y, x].type == ZoneType.HorizontalRiver ||
                    map[y, x].type == ZoneType.VerticalRiver)
                {
                    continue;
                }

                int dx = Mathf.Abs(startPos.x - x);
                int dy = Mathf.Abs(startPos.y - y);

                int GrandmaDx = Mathf.Abs(grandmaPos.x - x);
                int GrandmaDy = Mathf.Abs(grandmaPos.y - y);

                if (dx >= minAxisDistance &&
                    dy >= minAxisDistance &&
                    GrandmaDx >= minAxisDistance &&
                    GrandmaDy >= minAxisDistance)
                {
                    possiblePositions.Add(pos);
                }
            }
        }

        wolfDenPos = possiblePositions[
            Random.Range(0, possiblePositions.Count)
        ];

        map[wolfDenPos.y, wolfDenPos.x] = new WolfDen();
    }

    private void FillOtherZones()
    {
        PutWolfDenPos(startPos, grandmaPos);

        List<ZoneType> allowedFillTypes =
            new();

        int campZones = 0;

        foreach (
            ZoneType type
            in System.Enum.GetValues(
                typeof(ZoneType)))
        {
            if (
                type != ZoneType.MomHouse &&
                type != ZoneType.GrandmaHouse &&
                type != ZoneType.VerticalRiver &&
                type != ZoneType.HorizontalRiver &&
                type != ZoneType.WolfDen)
            {
                allowedFillTypes.Add(type);
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (map[y, x] != null)
                {
                    continue;
                }

                int randomIndex =
                    Random.Range(
                        0,
                        allowedFillTypes.Count
                    );

                ZoneType selectedType =
                    allowedFillTypes[randomIndex];

                if (campZones >= 5 && selectedType == ZoneType.Camp)
                {
                    x--;
                    continue;
                }

                if (selectedType == ZoneType.Camp)
                    campZones++;

                map[y, x] =
                    ZoneVault.GenerateZone(
                        selectedType
                    );
            }
        }
    }

    private void ImplementRiver(Vector2Int startPos)
    {
        int minX = Mathf.Min(startPos.x, grandmaPos.x);
        int maxX = Mathf.Max(startPos.x, grandmaPos.x);

        int minY = Mathf.Min(startPos.y, grandmaPos.y);
        int maxY = Mathf.Max(startPos.y, grandmaPos.y);

        List<bool> possibleOrientations = new();

        if (minY + 1 < maxY)
            possibleOrientations.Add(true);

        if (minX + 1 < maxX)
            possibleOrientations.Add(false);

        if (possibleOrientations.Count == 0)
        {
            return;
        }

        bool horizontalRiver =
            possibleOrientations[
                Random.Range(0, possibleOrientations.Count)
            ];

        if (horizontalRiver)
        {
            int riverY = Random.Range(minY + 1, maxY);

            for (int x = 0; x < mapWidth; x++)
            {
                PlaceRiverTile(x, riverY, startPos, ZoneType.HorizontalRiver);
            }
        }
        else
        {
            int riverX = Random.Range(minX + 1, maxX);

            for (int y = 0; y < mapHeight; y++)
            {
                PlaceRiverTile(riverX, y, startPos, ZoneType.VerticalRiver);
            }
        }
    }

    private bool HasEnoughAccessibleTilesBeforeRiver()
    {
        bool[,] visited = new bool[mapHeight, mapWidth];
        Queue<Vector2Int> queue = new();

        queue.Enqueue(startPos);
        visited[startPos.y, startPos.x] = true;

        int accessibleTiles = 0;

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            accessibleTiles++;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (next.x < 0 || next.x >= mapWidth ||
                    next.y < 0 || next.y >= mapHeight)
                    continue;

                if (visited[next.y, next.x])
                    continue;

                if (map[next.y, next.x] != null &&
                    (map[next.y, next.x].type == ZoneType.VerticalRiver || map[next.y, next.x].type == ZoneType.HorizontalRiver))
                    continue;

                visited[next.y, next.x] = true;
                queue.Enqueue(next);
            }
        }

        return accessibleTiles >= MinAccessibleTilesBeforeRiver;
    }

    private void PlaceRiverTile(
        int x,
        int y,
        Vector2Int startPos,
        ZoneType riverType)
    {
        Vector2Int pos =
            new(x, y);

        if (
            pos != startPos &&
            pos != grandmaPos)
        {
            map[y, x] =
                ZoneVault.GenerateZone(
                    riverType
                );
        }
    }

    private void PrintMap()
    {
        StringBuilder mapLayout =
            new();

        mapLayout.AppendLine(
            "\n=== MAP GRID (7x7) ==="
        );

        for (
            int y = mapHeight - 1;
            y >= 0;
            y--)
        {
            for (
                int x = 0;
                x < mapWidth;
                x++)
            {
                Vector2Int currentPos =
                    new(x, y);

                if (
                    player1 != null &&
                    currentPos ==
                    player1.NetworkMapPos.Value)
                {
                    mapLayout.Append("[P]");
                }
                else if (
                    currentPos == grandmaPos)
                {
                    mapLayout.Append("[A]");
                }
                else if (
                    map[y, x] != null)
                {
                    string zoneName =
                        map[y, x]
                            .GetType()
                            .Name;

                    string tag =
                        zoneName.Length >= 2
                            ? zoneName.Substring(
                                0,
                                2)
                            : zoneName.PadRight(2);

                    mapLayout.Append(
                        $"[{tag}]"
                    );
                }
                else
                {
                    mapLayout.Append("[??]");
                }
            }

            mapLayout.AppendLine();
        }

        LogManager.Log(
            mapLayout.ToString()
        );
    }

    public void OnSwapingZone(
        Player player,
        Direction direction,
        Side side)
    {
        PlayerMoveResult moveResult =
            CanSwapZone(
                player,
                direction
            );

        if (!moveResult.canMove)
        {
            LogManager.Log(
                $"Cannot move {direction}. Out of bounds."
            );

            return;
        }

        Vector2Int newPos =
            moveResult.newPos;

        UIManager.Instance.PlayZoneTransition(
            () =>
            {
                Vector3 entryPosition =
                    GetPlayerEntryPoint(
                        player,
                        direction,
                        newPos,
                        side
                    );

                player.transform.position = entryPosition;

                player.SetNetworkMapPosServerRpc(
                    newPos,
                    direction,
                    entryPosition
                );
            },
            player
        );
    }

    private Vector3 GetPlayerEntryPoint(
        Player player,
        Direction direction,
        Vector2Int newPos,
        Side side)
    {
        ZoneData currentZoneData =
            map[newPos.y, newPos.x];

        Vector3[] entryPoints =
            currentZoneData.GetPlayerEntryPoint(
                player == player1
            );

        return currentZoneData.GetEntryPoint(
            direction,
            entryPoints,
            side
        );
    }

    private PlayerMoveResult CanSwapZone(
        Player player,
        Direction direction)
    {
        Vector2Int newPos =
            player.NetworkMapPos.Value;

        switch (direction)
        {
            case Direction.North:
                newPos.y++;
                break;

            case Direction.South:
                newPos.y--;
                break;

            case Direction.East:
                newPos.x++;
                break;

            case Direction.West:
                newPos.x--;
                break;
        }

        return new PlayerMoveResult(
            newPos.x >= 0 &&
            newPos.x < mapWidth &&
            newPos.y >= 0 &&
            newPos.y < mapHeight,
            newPos
        );
    }

    public void UpdateChestUINetworking(ulong actualPlayerId, Vector2Int position)
    {
        map[position.y, position.x].chestOpened = true;

        Player actualPlayer = GetPlayerById(actualPlayerId) == player1 ? player1 : player2;
        Player otherPlayer = GetOtherPlayer(actualPlayer);

        UpdateChestSprite(actualPlayer, position);
        UpdateChestSprite(otherPlayer, position);
    }

    private void UpdateChestSprite(Player player, Vector2Int position)
    {
        if (player == null)
            return;

        if (player.NetworkMapPos.Value != position)
            return;

        Chest[] chests = player.currentZone.GetComponentsInChildren<Chest>();

        foreach (var chest in chests)
        {
            chest.GetComponent<SpriteRenderer>().sprite =
                Resources.Load<Sprite>("Prefabs/Objects/OpenedBox");
        }
    }

    private struct PlayerMoveResult
    {
        public bool canMove;
        public Vector2Int newPos;

        public PlayerMoveResult(
            bool canMove,
            Vector2Int newPos)
        {
            this.canMove = canMove;
            this.newPos = newPos;
        }
    }
}