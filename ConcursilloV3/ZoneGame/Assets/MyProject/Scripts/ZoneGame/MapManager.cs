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

    //[SerializeField] private Vector3 player1ZonePos;
    //[SerializeField] private Vector3 player2ZonePos;

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

            LogManager.Log(
                $"Player 1 registrado: {player.name}"
            );
        }
        else if (player2 == null)
        {
            player2 = player;
            player2.name = "Player 2";

            LogManager.Log(
                $"Player 2 registrado: {player.name}"
            );
        }

        if (player1 != null &&
            player2 != null &&
            isServer)
        {
            LogManager.Log(
                "Los dos Players están registrados. Generando mapa..."
            );

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
        InitPlayersInfo(startPos);

        map[startPos.y, startPos.x] =
            new MomHouse();

        map[grandmaPos.y, grandmaPos.x] =
            new GrandmaHouse();

        ImplementRiver(startPos);
        FillOtherZones();

        player1.NetworkMapPos.Value = startPos;
        player2.NetworkMapPos.Value = startPos;

        PrintMap();

        CreateInitialZones();

        IsMapReady = true;

        StartCoroutine(SendMapNextFrame());
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

        LogManager.Log(
            "Mapa recibido del servidor."
        );

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

            ZoneType.River =>
                new River(),

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

    private Vector2Int GrandmaHousePos(
        Vector2Int startPos)
    {
        int targetDistance = 7;

        List<Vector2Int> possiblePositions =
            new();

        while (
            possiblePositions.Count == 0 &&
            targetDistance > 0)
        {
            possiblePositions.Clear();

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int distance =
                        Mathf.Abs(
                            startPos.x - x
                        ) +
                        Mathf.Abs(
                            startPos.y - y
                        );

                    if (distance >= targetDistance)
                    {
                        possiblePositions.Add(
                            new Vector2Int(x, y)
                        );
                    }
                }
            }

            targetDistance--;
        }

        return possiblePositions[
            Random.Range(
                0,
                possiblePositions.Count
            )
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
                type != ZoneType.River)
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

    private void ImplementRiver(
        Vector2Int startPos)
    {
        int midX = Mathf.Clamp(
            (startPos.x + grandmaPos.x) / 2,
            1,
            mapWidth - 2
        );

        int midY = Mathf.Clamp(
            (startPos.y + grandmaPos.y) / 2,
            1,
            mapHeight - 2
        );

        int riverStyle =
            Random.Range(0, 2);

        switch (riverStyle)
        {
            case 0:

                for (int x = 0; x < mapWidth; x++)
                {
                    PlaceRiverTile(
                        x,
                        midY,
                        startPos
                    );
                }

                break;

            case 1:

                for (int y = 0; y < mapHeight; y++)
                {
                    PlaceRiverTile(
                        midX,
                        y,
                        startPos
                    );
                }

                break;
        }
    }

    private void PlaceRiverTile(
        int x,
        int y,
        Vector2Int startPos)
    {
        Vector2Int pos =
            new(x, y);

        if (
            pos != startPos &&
            pos != grandmaPos)
        {
            map[y, x] =
                ZoneVault.GenerateZone(
                    ZoneType.River
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

        LogManager.Log(
            $"Chest at {position} opened!"
        );
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