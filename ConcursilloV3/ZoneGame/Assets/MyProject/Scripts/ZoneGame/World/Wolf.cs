using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[RequireComponent(typeof(WolfUI))]
public class Wolf : NetworkBehaviour
{
    private const int moveInterval = 10;
    private const int distanceToChasePlayer = 3;
    private const int distanceToChaseMeat = 5;
    private const int maxCountToSteal = 4;
    private const int maxItemsToSteal = 2;

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

            player1.NetworkMapPos.OnValueChanged += UpdateWolfState;
            player2.NetworkMapPos.OnValueChanged += UpdateWolfState;

            NetworkPosition.Value = MapManager.Instance.wolfDenPos;
            readyToMove = true;
        }
    }

    public void UpdateWolfState(Vector2Int oldPos, Vector2Int newPos)
    {
        if (wolfState.Value == WolfState.MovingToDen)
            return;

        Player target = CanMoveToPlayer();

        wolfState.Value = target == null 
            ? WolfState.MovingRandom
            : WolfState.MovingToPlayer;
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

    public void SetReadyToMove(bool status)
    {
        readyToMove = status;
    }

    private void MoveWolf()
    {
        Debug.Log($"Wolf before moving -> Y: {NetworkPosition.Value.y}, X: {NetworkPosition.Value.x}");

        if (wolfState.Value == WolfState.MovingToDen)
        {
            MoveToDen();
            return;
        }

        Player playerToChase = CanMoveToPlayer();

        int option = (playerToChase != null) switch
        {
            (true) => 1,
            _ => 2
        };

        switch (option)
        {
            case 1:
                MoveToPlayer(playerToChase);
                break;
            default:
                NetworkPosition.Value = MoveRandomly();
                break;
        }

        Debug.Log($"Wolf after moving -> Y: {NetworkPosition.Value.y}, X: {NetworkPosition.Value.x}");

        Player[] playersCaught = CheckPlayerCollision();

        if (playersCaught == null)
            return;

        foreach (var player in playersCaught)
        {
            StealFromPlayer(player);
        }

        wolfState.Value = WolfState.MovingToDen;
    }

    private void StealFromPlayer(Player player)
    {
        if (player.State.Value == PlayerState.Transitioning || player.State.Value == PlayerState.EndGame)
            return;

        int items = ItemsToSteal(player);

        ItemsStolenResult itemsStolen = StealingItems(items, player);

        player.SetInventory();

        StealFromPlayerRpc(
            itemsStolen.items.ToArray(),
            itemsStolen.amount.ToArray(),
            itemsStolen.stealResult,
            RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp)
        );
    }

    private int ItemsToSteal(Player player)
    {
        ItemType itemType = player.inventory.TryToGet(ItemType.LuckPotion, ItemVault.IsBuff(ItemType.LuckPotion));
        
        return Random.Range(0, 100) < ItemVault.GetBuffValue(itemType, BuffApplierType.Wolf) ? 0 : Random.Range(1, maxItemsToSteal + 1);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void StealFromPlayerRpc(
        ItemType[] items,
        int[] amounts,
        StealResult stealResult,
        RpcParams rpcParams = default)
    {
        Player localPlayer = MapManager.Instance.GetLocalPlayer();

        if (localPlayer == null)
            return;

        wolfUI.ShowItemsStolen(items, amounts, localPlayer, stealResult);
    }

    private ItemsStolenResult StealingItems(
        int itemsToSteal,
        Player player)
    {
        List<ItemType> stolenItems = new();
        List<int> amount = new();
        HashSet<ItemType> stolenItemTypes = new();
        StealResult stealResult = StealResult.Success;

        if (itemsToSteal <= 0)
            return new ItemsStolenResult(stolenItems, amount, StealResult.LuckPotionEffect);

        if (player.inventory.GetAmount(ItemType.Meat) > 0)
        {
            player.inventory.Remove(ItemType.Meat, 1);

            return new ItemsStolenResult(
                new List<ItemType> { ItemType.Meat },
                new List<int> { 1 },
                StealResult.Success
            );
        }

        for (int i = 0; i < itemsToSteal; i++)
        {
            ItemType[] validItems = player.inventory.items.Keys
                .Where(item => ItemVault.IsStealable(item) && !stolenItemTypes.Contains(item))
                .ToArray();

            if (validItems.Length == 0)
            {
                stealResult = StealResult.NoItemsToSteal;
                break;
            }

            ItemType[] validItemsInWolfDen = validItems
                .Where(item => SpaceInWolfDenChest(item))
                .ToArray();

            if (validItemsInWolfDen.Length == 0)
            {
                stealResult = StealResult.ChestFull;
                break;
            }

            ItemType item = ItemVault.GetRandomWeightedItem(validItemsInWolfDen);

            if (item == ItemType.None)
                continue;

            int availableAmount = player.inventory.GetAmount(item);

            int amountToSteal = Random.Range(
                1,
                Mathf.Min(ItemVault.MaxAmountOf(item, maxCountToSteal), availableAmount) + 1
            );

            if (!player.inventory.Remove(item, amountToSteal))
                continue;

            if (!AddItemsToWolfDenChest(item, amountToSteal, player))
                continue;

            stolenItemTypes.Add(item);
            stolenItems.Add(item);
            amount.Add(amountToSteal);
        }

        return new ItemsStolenResult(stolenItems, amount, stealResult);
    }

    private bool SpaceInWolfDenChest(ItemType item)
    {
        Vector2Int pos = MapManager.Instance.wolfDenPos;
        ZoneData wolfDen = map[pos.y, pos.x];

        return wolfDen.chestInventory.HasSpaceFor(item);
    }

    private bool AddItemsToWolfDenChest(ItemType item, int amount, Player player)
    {
        Vector2Int pos = MapManager.Instance.wolfDenPos;
        ZoneData wolfDen = map[pos.y, pos.x];

        if (!wolfDen.chestInventory.Add(item, amount))
        {
            player.inventory.Add(item, amount);
            return false;
        }

        player.SyncChestItemsRpc(
           new[] { pos },
           new[] { item },
           new[] { amount }
       );

        return true;
    }

    private void MoveToDen()
    {
        Vector2Int wolfDenPos = MapManager.Instance.wolfDenPos;
       
        Direction direction = MoveTowards(wolfDenPos);

        NetworkPosition.Value = MoveWolfTo(direction, wolfDenPos);

        if (NetworkPosition.Value == wolfDenPos)
            wolfState.Value = WolfState.MovingRandom;
    }

    /*
    private void MoveToMeat(Vector2Int meatPos)
    {
        wolfState.Value = WolfState.MovingToMeat;

        Direction direction = MoveTowards(meatPos);

        NetworkPosition.Value = MoveWolfTo(direction, meatPos);

        if (NetworkPosition.Value == meatPos)
        {
            RemoveMeatInZone(meatPos);
            wolfState.Value = WolfState.MovingToDen;
        }
    }
    */

    private void MoveToPlayer(Player player)
    {
        wolfState.Value = WolfState.MovingToPlayer;

        Direction direction = MoveTowards(player.NetworkMapPos.Value);

        NetworkPosition.Value = MoveWolfTo(direction, player.NetworkMapPos.Value);
    }

    private Vector2Int MoveRandomly()
    {
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

        Debug.LogError("El lobo no tiene movimientos disponibles.");

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

    /*
    private void RemoveMeatInZone(Vector2Int meatPos)
    {
        ZoneData zone = map[meatPos.y, meatPos.x];

        zone.chestInventory.Remove(ItemType.Meat, 1);

        RemoveMeatInZoneClientRpc(meatPos);
    }

    [Rpc(SendTo.NotServer)]
    private void RemoveMeatInZoneClientRpc(Vector2Int meatPos)
    {
        ZoneData zone = map[meatPos.y, meatPos.x];

        zone.chestInventory.Remove(ItemType.Meat, 1);
    }
    */

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

        Debug.LogError($"El lobo no encuentra una ruta hasta {targetPos}.");

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

    /*
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
                    zone.chestInventory.GetAmount(ItemType.Meat) <= 0)
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
    */

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
        bool player1CanBeCaught =
            player1.NetworkMapPos.Value == NetworkPosition.Value &&
            CanPlayerBeCaught(player1);

        bool player2CanBeCaught =
            player2.NetworkMapPos.Value == NetworkPosition.Value &&
            CanPlayerBeCaught(player2);

        if (player1CanBeCaught && player2CanBeCaught)
            return new[] { player1, player2 };

        if (player1CanBeCaught)
            return new[] { player1 };

        if (player2CanBeCaught)
            return new[] { player2 };

        return null;
    }

    private bool CanPlayerBeCaught(Player player)
    {
        if (player.State.Value == PlayerState.Transitioning ||
            player.State.Value == PlayerState.EndGame)
            return false;

        int protection = ItemVault.GetBuffValue(
            player.inventory.TryToGet(ItemType.AncestralNecklace, ItemVault.IsBuff(ItemType.AncestralNecklace)),
            BuffApplierType.Wolf
        );

        return Random.Range(0, 100) >= protection;
    }

    private struct ItemsStolenResult
    {
        public List<ItemType> items;
        public List<int> amount;

        public StealResult stealResult;

        public ItemsStolenResult(List<ItemType> items, List<int> amount, StealResult stealResult = StealResult.Success)
        {
            this.items = items;
            this.amount = amount;
            this.stealResult = stealResult;
        }
    }
}

public enum WolfState
{
    MovingRandom,
    MovingToPlayer,
    MovingToDen
    //MovingToMeat,
}

public enum StealResult
{
    Success,
    NoItemsToSteal,
    ChestFull,
    LuckPotionEffect
}