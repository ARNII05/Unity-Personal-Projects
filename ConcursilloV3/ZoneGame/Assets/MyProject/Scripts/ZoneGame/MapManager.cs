using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager instance { get; private set; }
    public bool IsMapReady { get; private set; }

    public ZoneData[,] map;
    public Vector2Int grandmaPos;

    private Vector2Int startPos;

    public const int mapWidth = 7;
    public const int mapHeight = 7;
    private const int MinAccessibleTilesBeforeRiver = 10;

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
    }

    private void Start()
    {
        map = new ZoneData[mapHeight, mapWidth];
        IsMapReady = false;
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
            GenerateStartPositions();
            MakeRandomMap();
        }
    }

    private void GenerateStartPositions()
    {
        startPos = new Vector2Int(
            Random.Range(0, mapWidth),
            Random.Range(0, mapHeight)
        );

        grandmaPos = GrandmaHousePos(startPos);
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
                IsMapReady = true;
                StartCoroutine(SendMapNextFrame());

                return;
            }
        }

        Debug.LogError(
            "No se pudo generar un mapa válido después de 100 intentos."
        );
    }

    private System.Collections.IEnumerator SendMapNextFrame()
    {
        yield return null;

        SendMapToClient();
    }

    private void SendMapToClient()
    {
        NetworkZoneData[] networkMap =
            new NetworkZoneData[mapWidth * mapHeight];

        int index = 0;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                networkMap[index] =
                    new NetworkZoneData(map[y, x]);

                index++;
            }
        }

        player1.SendMapClientRpc(
            networkMap,
            startPos,
            grandmaPos
        );
    }

    public void ReceiveMapFromServer(
        NetworkZoneData[] networkMap,
        Vector2Int serverStartPos,
        Vector2Int serverGrandmaPos)
    {
        map = new ZoneData[mapHeight, mapWidth];

        startPos = serverStartPos;
        grandmaPos = serverGrandmaPos;

        int index = 0;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                map[y, x] =
                    CreateZoneFromNetworkData(
                        networkMap[index]
                    );

                index++;
            }
        }

        InitPlayersInfo(startPos);

        CreateInitialZones();
        IsMapReady = true;

        PrintMap();
    }

    private ZoneData CreateZoneFromNetworkData(
        NetworkZoneData networkData)
    {
        ZoneData zone = networkData.type switch
        {
            ZoneType.MomHouse =>
                new MomHouse(),

            ZoneType.Forest =>
                new Forest(),

            ZoneType.SpecialZone =>
                new SpecialZone(),

            ZoneType.WaterPit =>
                new WaterPit(),

            ZoneType.FlowerField =>
                new FlowerField(),

            ZoneType.Ruins =>
                new Ruins(),

            ZoneType.Swamp =>
                new Swamp(),

            ZoneType.Village =>
                new Village(),

            ZoneType.Camp =>
                new Camp(),

            ZoneType.VerticalRiver =>
                new VerticalRiver(),

            ZoneType.HorizontalRiver => 
                new HorizontalRiver(),

            ZoneType.GrandmaHouse =>
                new GrandmaHouse(),

            _ => null
        };

        if (zone == null)
        {
            Debug.LogError(
                $"No se pudo crear la zona {networkData.type}"
            );

            return null;
        }

        zone.chestOpened =
            networkData.chestOpened;

        zone.flowersRemaining =
            networkData.flowersRemaining;

        zone.riverBridged =
            networkData.riverBridged;

        return zone;
    }

    private void InitPlayersInfo(Vector2Int startPos)
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
    }

    private void CreateInitialZones()
    {
        if (player1 == null ||
            player2 == null)
        {
            return;
        }

        Vector3 p1ZonePos = map[startPos.y, startPos.x].player1ZonePos;
        Vector3 p2ZonePos = map[startPos.y, startPos.x].player2ZonePos;

        CreatePlayerZone(
            player1,
            p1ZonePos
        );

        CreatePlayerZone(
            player2,
            p2ZonePos
        );
    }

    private void CreatePlayerZone(
        Player player,
        Vector3 zonePosition)
    {
        if (player.currentZone != null)
        {
            Destroy(player.currentZone);
        }

        Vector2Int position = player.NetworkMapPos.Value;

        ZoneData zoneData =
            map[position.y, position.x];

        GameObject prefab =
            LoadZone(zoneData.type);

        if (prefab == null)
        {
            Debug.LogError(
                "No se pudo cargar el prefab MomHouse."
            );

            return;
        }

        GameObject zone =
            Instantiate(
                prefab,
                zonePosition,
                Quaternion.identity
            );

        player.currentZone = zone;

        Zone currentZone =
            player.currentZone
                .GetComponent<Zone>();

        currentZone.Setup(
            player.NetworkMapPos.Value,
            map
        );
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

    private void FillOtherZones()
    {
        List<ZoneType> allowedFillTypes =
            new();

        foreach (
            ZoneType type
            in System.Enum.GetValues(
                typeof(ZoneType)))
        {
            if (
                type != ZoneType.MomHouse &&
                type != ZoneType.GrandmaHouse &&
                type != ZoneType.VerticalRiver &&
                type != ZoneType.HorizontalRiver)
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
        Direction direction)
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

        player.transform.position =
            GetPlayerEntryPoint(
                player,
                direction,
                newPos
            );

        player.SetNetworkMapPosServerRpc(
            newPos
        );

        LogManager.Log(
            $"{player.name} entered zone " +
            $"NetworkMapPos: {newPos}"
        );
    }

    private Vector3 GetPlayerEntryPoint(
        Player player,
        Direction direction,
        Vector2Int newPos)
    {
        ZoneData currentZoneData =
            map[newPos.y, newPos.x];

        Vector3[] entryPoints =
            currentZoneData.GetPlayerEntryPoint(
                player == player1
            );

        return currentZoneData.GetEntryPoint(
            direction,
            entryPoints
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

    public void UpdateActualZone(
        Player player)
    {
        if (map == null ||
            player == null)
        {
            return;
        }

        if (player.currentZone != null)
        {
            Destroy(player.currentZone);
        }

        Vector2Int position = player.NetworkMapPos.Value;

        ZoneData zoneData =
            map[position.y, position.x];

        GameObject prefab =
            LoadZone(zoneData.type);

        player.currentZone =
            Instantiate(
                prefab,
                player == player1
                    ? zoneData.player1ZonePos
                    : zoneData.player2ZonePos,
                Quaternion.identity
            );

        Zone currentZone =
            player.currentZone
                .GetComponent<Zone>();

        currentZone.Setup(
            player.NetworkMapPos.Value,
            map
        );
    }

    public void OnChestOpen(
        Vector2Int position)
    {
        map[position.y, position.x]
            .chestOpened = true;
    }

    public void OnChestOpenedNetworked(
        Vector2Int position)
    {
        map[position.y, position.x]
            .chestOpened = true;
    }

    public void DisableChestAtPosition(
        Vector2Int position,
        Player player)
    {
        if (
            player.NetworkMapPos.Value !=
            position)
        {
            return;
        }

        if (player.currentZone == null)
        {
            return;
        }

        Zone zone =
            player.currentZone
                .GetComponent<Zone>();

        zone.DisableChest();
    }
}