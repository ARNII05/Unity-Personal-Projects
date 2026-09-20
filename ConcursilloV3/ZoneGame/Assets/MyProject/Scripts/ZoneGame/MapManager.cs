using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class MapManager : MonoBehaviour
{
    public static MapManager instance { get; private set; }

    public ZoneData[,] map;
    public Vector2Int grandmaPos;
    private Vector2Int startPos;

    public const int mapWidth = 7;
    public const int mapHeight = 7;
    [SerializeField] private Vector3 player1ZonePos;
    [SerializeField] private Vector3 player2ZonePos;

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

        startPos = new(
            Random.Range(0, mapWidth),
            Random.Range(0, mapHeight)
        );
    }

    public void RegisterPlayer(Player player)
    {
        if (player1 == null)
        {
            player1 = player;
            player1.inventory = new Inventory();
            player1.name = "Player 1";
            Debug.Log($"Player 1 registrado: {player.name}");
        }
        else if (player2 == null)
        {
            player2 = player;
            player2.inventory = new Inventory();
            player2.name = "Player 2";
            Debug.Log($"Player 2 registrado: {player.name}");
        }

        if (player1 != null && player2 != null)
        {
            Debug.Log("Los dos Players están registrados. Generando mapa...");
            MakeRandomMap();
        }
    }

    public void MakeRandomMap()
    {   
        InitPlayersInfo(startPos);

        map[startPos.y, startPos.x] = new MomHouse();

        grandmaPos = GrandmaHousePos(startPos);

        map[grandmaPos.y, grandmaPos.x] = new GrandmaHouse();

        ImplementRiver(startPos);

        FillOtherZones();

        player1.NetworkMapPos.Value = startPos;
        player2.NetworkMapPos.Value = startPos;

        Debug.Log($"Starting position for both players: {startPos}");
        Debug.Log($"Player 1 initial position: {player1.NetworkMapPos.Value}");
        Debug.Log($"Player 2 initial position: {player2.NetworkMapPos.Value}");

        PrintMap();
    }

    private void InitPlayersInfo(Vector2Int startPos)
    {
        player1.initialPos = startPos;
        player2.initialPos = startPos;
        player1.transform.position = player1RealPos;
        player2.transform.position = player2RealPos;

        GameObject player1Zone = Instantiate(
            Resources.Load<GameObject>(ZonePrefabPath + "MomHouse"),
            player1ZonePos,
            Quaternion.identity
        );

        GameObject player2Zone = Instantiate(
            Resources.Load<GameObject>(ZonePrefabPath + "MomHouse"),
            player2ZonePos,
            Quaternion.identity
        );

        player1.currentZone = player1Zone;
        player2.currentZone = player2Zone;
    }

    private Vector2Int GrandmaHousePos(Vector2Int startPos)
    {
        int targetDistance = 7;
        List<Vector2Int> possiblePositions = new();

        while (possiblePositions.Count == 0 && targetDistance > 0)
        {
            possiblePositions.Clear();

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int distance =
                        Mathf.Abs(startPos.x - x) +
                        Mathf.Abs(startPos.y - y);

                    if (distance >= targetDistance)
                    {
                        possiblePositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            targetDistance--;
        }

        return possiblePositions[Random.Range(0, possiblePositions.Count)];
    }

    private void FillOtherZones()
    {
        List<ZoneType> allowedFillTypes = new();

        foreach (ZoneType type in System.Enum.GetValues(typeof(ZoneType)))
        {
            if (type != ZoneType.MomHouse &&
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
                if (map[y, x] != null) continue;

                int randomIndex = Random.Range(0, allowedFillTypes.Count);
                ZoneType selectedType = allowedFillTypes[randomIndex];

                map[y, x] = ZoneVault.GenerateZone(selectedType);
            }
        }
    }

    private void ImplementRiver(Vector2Int startPos)
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

        int riverStyle = Random.Range(0, 2);

        switch (riverStyle)
        {
            case 0:
                for (int x = 0; x < mapWidth; x++)
                {
                    PlaceRiverTile(x, midY, startPos);
                }
                break;

            case 1:
                for (int y = 0; y < mapHeight; y++)
                {
                    PlaceRiverTile(midX, y, startPos);
                }
                break;
        }
    }

    private void PlaceRiverTile(int x, int y, Vector2Int startPos)
    {
        Vector2Int pos = new(x, y);

        if (pos != startPos && pos != grandmaPos)
        {
            map[y, x] = ZoneVault.GenerateZone(ZoneType.River);
        }
    }

    private void PrintMap()
    {
        StringBuilder mapLayout = new();
        mapLayout.AppendLine("\n=== MAP GRID (7x7) ===");

        for (int y = mapHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2Int currentPos = new(x, y);

                if (currentPos == player1.NetworkMapPos.Value)
                {
                    mapLayout.Append("[P]");
                }
                else if (currentPos == grandmaPos)
                {
                    mapLayout.Append("[A]");
                }
                else if (map[y, x] != null)
                {
                    string zoneName = map[y, x].GetType().Name;
                    string tag = zoneName.Length >= 2
                        ? zoneName.Substring(0, 2)
                        : zoneName.PadRight(2);

                    mapLayout.Append($"[{tag}]");
                }
                else
                {
                    mapLayout.Append("[??]");
                }
            }

            mapLayout.AppendLine();
        }

        LogManager.Log(mapLayout.ToString());
    }

    public void OnSwapingZone(
        Player player,
        Direction direction)
    {
        PlayerMoveResult moveResult =
            CanSwapZone(player, direction);

        if (!moveResult.canMove)
        {
            LogManager.Log(
                $"Cannot move {direction}. Out of bounds."
            );

            return;
        }

        Vector2Int newPos = moveResult.newPos;

        player.transform.position =
            GetPlayerEntryPoint(
                player,
                direction,
                newPos
            );

        player.SetNetworkMapPosServerRpc(newPos);

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

    private PlayerMoveResult CanSwapZone(Player player, Direction direction)
    {
        Vector2Int newPos = player.NetworkMapPos.Value;

        switch (direction)
        {
            case Direction.North:
                newPos.y += 1;
                break;

            case Direction.South:
                newPos.y -= 1;
                break;

            case Direction.East:
                newPos.x += 1;
                break;

            case Direction.West:
                newPos.x -= 1;
                break;
        }

        return new PlayerMoveResult(
            newPos.x >= 0 && newPos.x < mapWidth &&
            newPos.y >= 0 && newPos.y < mapHeight,
            newPos
        );
    }

    struct PlayerMoveResult
    {
        public bool canMove;
        public Vector2Int newPos;

        public PlayerMoveResult(bool canMove, Vector2Int newPos)
        {
            this.canMove = canMove;
            this.newPos = newPos;
        }
    }

    public void UpdateActualZone(Player player)
    {
        if (player.currentZone != null)
        {
            Destroy(player.currentZone);
        }

        player.currentZone = Instantiate(
            Resources.Load<GameObject>(ZonePrefabPath + "MomHouse"),
            player == player1 ? player1ZonePos : player2ZonePos,
            Quaternion.identity
        );

        Zone currentZone = player.currentZone.GetComponent<Zone>();
        currentZone.Setup(player.NetworkMapPos.Value, map);
    }

    public void OnChestOpen(Vector2Int position)
    {
        map[position.y, position.x].chestOpened = true;

        LogManager.Log($"Chest at {position} opened!");
    }

    public void OnChestOpenedNetworked(Vector2Int position)
    {
        map[position.y, position.x].chestOpened = true;
    }

    public void DisableChestAtPosition(Vector2Int position, Player player)
    {
        if (player.NetworkMapPos.Value != position)
            return;

        Zone zone = player.currentZone.GetComponent<Zone>();
        zone.DisableChest();
    }
}