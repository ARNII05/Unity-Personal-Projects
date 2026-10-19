using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;
using Random = UnityEngine.Random;

[RequireComponent(typeof(WolfUI))]
public class Wolf : NetworkBehaviour
{
    private const int moveInterval = 10;
    private const int distanceToChasePlayer = 3;
    private const int distanceToChaseMeat = 5;
    private const int maxCountToSteal = 2;
    private const int maxItemsToSteal = 4;

    private int mapWidth;
    private int mapHeight;

    private float time = moveInterval;

    private Player player1, player2;

    private ZoneData[,] map;

    private WolfUI wolfUI;

    public NetworkVariable<Vector2Int> NetworkPosition = new(
        Vector2Int.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<WolfState> wolfState = new(
        WolfState.MovingRandom,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool readyToMove = false;

    public void Init()
    {
        wolfUI = GetComponent<WolfUI>();
        map = MapManager.Instance.map;
        mapHeight = MapManager.mapHeight;
        mapWidth = MapManager.mapWidth;

        if (IsServer)
        {
            player1 = MapManager.Instance.Player1;
            player2 = MapManager.Instance.Player2;

            NetworkPosition.Value = MapManager.Instance.wolfDenPos;
            readyToMove = false;
        }
    }

    private void Update()
    {
        if (!IsServer || !readyToMove)
            return;

        time -= Time.deltaTime;

        if (time <= 0)
        {
            MoveWolf();
            time = moveInterval;
        }
    }

    private void MoveWolf()
    {
        if (wolfState.Value == WolfState.MovingToDen)
        {
            Debug.Log($"El lobo se está moviendo hacia su madriguera desde {NetworkPosition.Value.y}, {NetworkPosition.Value.x}");
            MoveToDen();
            Debug.Log($"Posición del lobo después de moverse: {NetworkPosition.Value.y}, {NetworkPosition.Value.x}");
            return;
        }

        Debug.Log($"Posición del lobo antes de moverse: {NetworkPosition.Value.y}, {NetworkPosition.Value.x}");

        Vector2Int meatPos = CanMoveToMeat();
        Player playerToChase = CanMoveToPlayer();

        int option = (meatPos.x != -1, playerToChase != null) switch
        {
            (true, _) => 1,
            (false, true) => 2,
            _ => 3
        };

        switch (option)
        {
            case 1:
                MoveToMeat(meatPos);
                break;
            case 2:
                MoveToPlayer(playerToChase);
                break;
            default:
                NetworkPosition.Value = MoveRandomly();
                break;
        }

        Player[] playersCaught = CheckPlayerCollision();

        if (playersCaught != null)
        {
            foreach (var player in playersCaught)
            {
                StealFromPlayer(player);
            }

            wolfState.Value = WolfState.MovingToDen;
        }

        Debug.Log($"Posición del lobo despúes de moverse: {NetworkPosition.Value.y}, {NetworkPosition.Value.x}");
    }

    private void StealFromPlayer(Player player)
    {
        ItemsStolenResult itemsStolen = ItemsToSteal(player);

        player.SetInventory();

        ClientRpcParams rpcParams = new()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { player.OwnerClientId }
            }
        };

        StealFromPlayerClientRpc(
            itemsStolen.items.ToArray(),
            itemsStolen.amount.ToArray(),
            itemsStolen.chestIsFull,
            player.NetworkObjectId,
            rpcParams
        );
    }

    [ClientRpc]
    private void StealFromPlayerClientRpc(
        ItemType[] items,
        int[] amounts,
        bool chestIsFull,
        ulong playerNetworkObjectId,
        ClientRpcParams rpcParams = default)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
            playerNetworkObjectId, out NetworkObject playerObject))
            return;

        if (!playerObject.TryGetComponent<Player>(out var player))
            return;

        Dictionary<ItemType, int> itemsStolenDic =
            SoltenItemsToDic(items, amounts);

        wolfUI.ShowItemsStolen(itemsStolenDic, player, chestIsFull);
    }

    private ItemsStolenResult ItemsToSteal(Player player)
    {
        int itemsToSteal = Random.Range(1, maxItemsToSteal + 1);

        return StealingItems(itemsToSteal, maxCountToSteal, player);
    }
    
    private Dictionary<ItemType, int> SoltenItemsToDic(ItemType[] items, int[] amounts)
    {
        Dictionary<ItemType, int> ItemsStolenDic = new();

        for (int i = 0; i < items.Length; i++)
        {
            if (ItemsStolenDic.ContainsKey(items[i]))
            {
                ItemsStolenDic[items[i]] += amounts[i];
                continue;
            }

            ItemsStolenDic[items[i]] = amounts[i];
        }

        return ItemsStolenDic;
    }

    private ItemsStolenResult StealingItems(
        int itemsToSteal,
        int maxCountToSteal,
        Player player)
    {
        List<ItemType> stolenItems = new();
        List<int> amount = new();
        HashSet<ItemType> forbbidenItems = new() { ItemType.Bouquet, ItemType.Log, 
            ItemType.Map, ItemType.Radar};
        bool chestIsFull = false;

        for (int i = 0; i < itemsToSteal; i++)
        {
            ItemType[] validItems = player.inventory.items.Keys
                .Where(item => !forbbidenItems.Contains(item))
                .ToArray();

            if (validItems.Length == 0)
                break;

            ItemType[] validItemsInWolfDen = validItems
                .Where(item => SpaceInWolfDenChest(item))
                .ToArray();

            if (validItemsInWolfDen.Length == 0)
            {
                chestIsFull = true;
                break;
            }

            int itemIndex = Random.Range(0, validItemsInWolfDen.Length);
            ItemType item = validItemsInWolfDen[itemIndex];

            int availableAmount = player.inventory.GetItemAmount(item);

            int amountToSteal = Random.Range(
                1,
                Mathf.Min(maxCountToSteal, availableAmount) + 1
            );

            if (!player.inventory.RemoveItem(item, amountToSteal))
                continue;

            if (!AddItemsToWolfDenChest(item, amountToSteal, player))
                continue;

            forbbidenItems.Add(item);
            stolenItems.Add(item);
            amount.Add(amountToSteal);
        }

        return new ItemsStolenResult(stolenItems, amount, chestIsFull);
    }

    private bool SpaceInWolfDenChest(ItemType item)
    {
        Vector2Int pos = MapManager.Instance.wolfDenPos;
        ZoneData wolfDen = map[pos.y, pos.x];

        return wolfDen.chestInventory.HasSpaceForItem(item);
    }

    private bool AddItemsToWolfDenChest(ItemType item, int amount, Player player)
    {
        Vector2Int pos = MapManager.Instance.wolfDenPos;
        ZoneData wolfDen = map[pos.y, pos.x];

        if (!wolfDen.chestInventory.AddItem(item, amount))
        {
            player.inventory.AddItem(item, amount);
            return false;
        }

        return true;
    }

    private void MoveToDen()
    {
        Vector2Int wolfDenPos = MapManager.Instance.wolfDenPos;

        Debug.Log($"El lobo se está moviendo hacia su cueva en la posicion: {wolfDenPos.y}, {wolfDenPos.x}");
       
        Direction direction = MoveTowards(wolfDenPos);

        NetworkPosition.Value = MoveWolfTo(direction, wolfDenPos);

        if (NetworkPosition.Value == wolfDenPos)
        {
            Debug.Log($"El lobo ha llegado a la cueva en la posicion: {wolfDenPos.y}, {wolfDenPos.x}");
            wolfState.Value = WolfState.MovingRandom;
        }
    }

    private void MoveToMeat(Vector2Int meatPos)
    {
        Debug.Log($"El lobo se está moviendo hacia una carne en la posicion: {meatPos.y}, {meatPos.x}");

        wolfState.Value = WolfState.MovingToMeat;

        Direction direction = MoveTowards(meatPos);

        NetworkPosition.Value = MoveWolfTo(direction, meatPos);

        if (NetworkPosition.Value == meatPos)
        {
            Debug.Log($"El lobo se ha comido la carne en la posicion: {meatPos.y}, {meatPos.x}");
            RemoveMeatInZone(meatPos);
            wolfState.Value = WolfState.MovingToDen;
        }
    }

    private void MoveToPlayer(Player player)
    {
        Debug.Log($"El lobo se está moviendo hacia {player.name} en la posicion: {player.NetworkMapPos.Value.y}, {player.NetworkMapPos.Value.x}");

        wolfState.Value = WolfState.MovingToPlayer;

        Direction direction = MoveTowards(player.NetworkMapPos.Value);

        NetworkPosition.Value = MoveWolfTo(direction, player.NetworkMapPos.Value);
    }

    private Vector2Int MoveRandomly()
    {
        Debug.Log("El lobo se está moviendo random");

        wolfState.Value = WolfState.MovingRandom;

        Direction[] directions =
        {
            Direction.North,
            Direction.South,
            Direction.East,
            Direction.West
        };

        for (int i = directions.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (directions[i], directions[j]) = (directions[j], directions[i]);
        }

        foreach (Direction direction in directions)
        {
            Vector2Int newPos = CanSwapZone(NetworkPosition.Value, direction);

            if (newPos.x == -1)
                continue;

            return newPos;
        }

        Debug.Log("El lobo no tiene movimientos disponibles.");

        return NetworkPosition.Value;
    }
    
    private Vector2Int MoveWolfTo(Direction direction, Vector2Int targetPos)
    {
        return direction switch
        {
            Direction.North => NetworkPosition.Value + Vector2Int.up,
            Direction.South => NetworkPosition.Value + Vector2Int.down,
            Direction.East => NetworkPosition.Value + Vector2Int.right,
            Direction.West => NetworkPosition.Value + Vector2Int.left,
            _ => NetworkPosition.Value == targetPos ? NetworkPosition.Value : MoveRandomly()
        };
    }

    private void RemoveMeatInZone(Vector2Int meatPos)
    {
        ZoneData zone = map[meatPos.y, meatPos.x];

        zone.chestInventory.RemoveItem(ItemType.Meat, 1);

        RemoveMeatInZoneClientRpc(meatPos);
    }

    [ClientRpc]
    private void RemoveMeatInZoneClientRpc(Vector2Int meatPos)
    {
        if (IsServer)
            return;
        
        ZoneData zone = map[meatPos.y, meatPos.x];

        zone.chestInventory.RemoveItem(ItemType.Meat, 1);
    }

    private Direction MoveTowards(Vector2Int targetPos)
    {
        Vector2Int startPos = NetworkPosition.Value;
        
        if (startPos == targetPos)
            return Direction.None;
        
        Queue<Vector2Int> positionsToVisit = new();

        Dictionary<Vector2Int, Vector2Int> previousPositions = new();

        positionsToVisit.Enqueue(startPos);
        previousPositions[startPos] = startPos;

        Direction[] directions =
        {
            Direction.North,
            Direction.South,
            Direction.East,
            Direction.West
        };

        while (positionsToVisit.Count > 0)
        {
            Vector2Int currentPos = positionsToVisit.Dequeue();

            foreach (Direction direction in directions)
            {
                Vector2Int move = CanSwapZone(currentPos, direction);
                
                if (move.x == -1)
                    continue;

                Vector2Int nextPos = move;

                if (previousPositions.ContainsKey(nextPos))
                    continue;
                
                previousPositions[nextPos] = currentPos;

                if (nextPos == targetPos)
                {
                    return GetFirstDirection(
                        startPos,
                        targetPos,
                        previousPositions
                    );
                }

                positionsToVisit.Enqueue(nextPos);
            }
        }

        Debug.LogWarning($"El lobo no encuentra una ruta hasta {targetPos}.");

        return Direction.None;
    }

    private Direction GetFirstDirection(
        Vector2Int startPos,
        Vector2Int targetPos,
        Dictionary<Vector2Int, Vector2Int> previousPositions)
    {
        Vector2Int currentPos = targetPos;

        while (previousPositions[currentPos] != startPos)
        {
            currentPos = previousPositions[currentPos];
        }

        Vector2Int difference = currentPos - startPos;

        if (difference == Vector2Int.up)
            return Direction.North;

        if (difference == Vector2Int.down)
            return Direction.South;

        if (difference == Vector2Int.right)
            return Direction.East;

        if (difference == Vector2Int.left)
            return Direction.West;

        return Direction.None;
    }

    private Vector2Int CanSwapZone(
        Vector2Int position,
        Direction direction)
    {
        Vector2Int newPos = position;

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

        bool canMove = newPos.x >= 0 &&
            newPos.x < mapWidth &&
            newPos.y >= 0 &&
            newPos.y < mapHeight &&
            map[newPos.y, newPos.x].type != ZoneType.Camp;

        return canMove ? newPos : new Vector2Int(-1, -1);
    }

    private Vector2Int CanMoveToMeat()
    {
        int closestDistance = int.MaxValue;
        Vector2Int closestPosition = new(-1, -1);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                ZoneData zone = map[y, x];

                if (zone.type == ZoneType.Camp ||
                    zone.type == ZoneType.WolfDen ||
                    zone.chestInventory.GetItemAmount(ItemType.Meat) <= 0)
                    continue;

                int distanceToMeat =
                    Mathf.Abs(x - NetworkPosition.Value.x) +
                    Mathf.Abs(y - NetworkPosition.Value.y);

                if (distanceToMeat > distanceToChaseMeat)
                    continue;

                if (distanceToMeat < closestDistance)
                {
                    closestDistance = distanceToMeat;
                    closestPosition = new Vector2Int(x, y);
                }
            }
        }

        return closestPosition;
    }

    private Player CanMoveToPlayer()
    {
        Vector2Int player1Pos = player1.NetworkMapPos.Value;
        Vector2Int player2Pos = player2.NetworkMapPos.Value;

        bool player1Safe =
            map[player1Pos.y, player1Pos.x].type == ZoneType.Camp;

        bool player2Safe =
            map[player2Pos.y, player2Pos.x].type == ZoneType.Camp;

        int distanceToPlayer1 =
            Mathf.Abs(player1Pos.x - NetworkPosition.Value.x) +
            Mathf.Abs(player1Pos.y - NetworkPosition.Value.y);

        int distanceToPlayer2 =
            Mathf.Abs(player2Pos.x - NetworkPosition.Value.x) +
            Mathf.Abs(player2Pos.y - NetworkPosition.Value.y);

        bool player1InRange =
            !player1Safe && distanceToPlayer1 <= distanceToChasePlayer;

        bool player2InRange =
            !player2Safe && distanceToPlayer2 <= distanceToChasePlayer;

        return (player1InRange, player2InRange, distanceToPlayer1 == distanceToPlayer2) switch
        {
            (false, false, _) => null,
            (false, _, _) => player2,
            (_, false, _) => player1,
            (_, _, true) => Random.Range(0, 2) == 0 ? player1 : player2,
            _ => distanceToPlayer1 < distanceToPlayer2 ? player1 : player2
        };
    }

    private Player[] CheckPlayerCollision()
    {
        Vector2Int p1Pos = player1.NetworkMapPos.Value;
        Vector2Int p2Pos = player2.NetworkMapPos.Value;

        bool isInSamePosWithP1 = p1Pos == NetworkPosition.Value;
        bool isInSamePosWithP2 = p2Pos == NetworkPosition.Value;

        return (isInSamePosWithP1, isInSamePosWithP2) switch
        {
            (true, true) => new[] { player1, player2 },
            (true, false) => new[] { player1 },
            (false, true) => new[] { player2 },
            _ => null
        }; 
    }
}

public enum WolfState
{
    MovingRandom,
    MovingToPlayer,
    MovingToMeat,
    MovingToDen
}

public struct ItemsStolenResult
{
    public List<ItemType> items;
    public List<int> amount;

    public bool chestIsFull;

    public ItemsStolenResult(List<ItemType> items, List<int> amount, bool chestIsFull = false)
    {
        this.items = items;
        this.amount = amount;
        this.chestIsFull = chestIsFull;
    }
}