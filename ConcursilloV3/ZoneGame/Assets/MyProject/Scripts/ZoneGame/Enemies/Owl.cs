using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Owl : NetworkBehaviour
{
    private bool readyToMove = false;
    private const int moveInterval = 20;

    private int mapWidth;
    private int mapHeight;

    private float time = moveInterval;

    private Player player1, player2;

    private ZoneData[,] map;

    private readonly HashSet<Vector2Int> cursedZones = new();

    private const int zonesPerCurse = 4;

    private bool isCurseActive = false, isPreparingCurse;

    public NetworkVariable<OwlState> State = new(
        OwlState.Resting,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    public void Init()
    {
        map = MapManager.Instance.map;
        mapHeight = MapManager.mapHeight;
        mapWidth = MapManager.mapWidth;

        if (IsServer)
        {
            player1 = MapManager.Instance.Player1;
            player2 = MapManager.Instance.Player2;

            readyToMove = true;
        }
    }

    private void Update()
    {
        if (!IsServer || !readyToMove || isCurseActive)
            return;

        time -= Time.deltaTime;

        if (time <= 0)
        {
            OrganizeCursedZones();
            time += moveInterval;
        }
    }

    private void OrganizeCursedZones()
    {
        if (isPreparingCurse)
            return;

        isPreparingCurse = true;
        StartCoroutine(OrganizeCursedZonesCoroutine());
    }

    private IEnumerator OrganizeCursedZonesCoroutine()
    {
        yield return new WaitUntil(() =>
            player1.State.Value != PlayerState.Transitioning &&
            player2.State.Value != PlayerState.Transitioning
        );

        Vector2Int startPos = new(
            Random.Range(0, mapWidth),
            Random.Range(0, mapHeight)
        );

        List<Vector2Int> generatedZones = GenerateCurseShape(startPos);

        if (generatedZones.Count != zonesPerCurse)
        {
            Debug.LogWarning("Owl could not generate the required cursed zones");
            isPreparingCurse = false;
            yield break;
        }

        bool addedAllFragments = DistributePurificationFragments(generatedZones);

        if (!addedAllFragments)
        {
            RemoveFragments();
            Debug.LogError($"Could not add all fragments to chest. Removed all.");
            isPreparingCurse = false;
            yield break;
        }

        isCurseActive = true;
        State.Value = OwlState.WaitingForPurification;

        ApplyCurseRpc(generatedZones.ToArray());

        isPreparingCurse = false;
    }

    private void RemoveFragments()
    {
        foreach (var zone in map)
        {
            int fragmentsAmount = zone.chestInventory.GetAmount(ItemType.PurificationFragment);

            if (fragmentsAmount > 0)
                zone.chestInventory.Remove(ItemType.PurificationFragment, fragmentsAmount);
        }

        Player[] players = new[] { player1, player2 };

        foreach (Player player in players)
        {
            int fragmentsAmount = player.inventory.GetAmount(ItemType.PurificationFragment);

            if (fragmentsAmount > 0)
                player.inventory.Remove(ItemType.PurificationFragment, fragmentsAmount);
        }
    }

    private bool DistributePurificationFragments(List<Vector2Int> generatedZones)
    {
        Vector2Int p1Pos = player1.NetworkMapPos.Value;
        Vector2Int p2Pos = player2.NetworkMapPos.Value;

        bool p1Inside = generatedZones.Contains(p1Pos);
        bool p2Inside = generatedZones.Contains(p2Pos);

        int totalFragments =
            CraftingSystem.purificationFragmentsForPurificationAmulet;

        int totalFragmentsInCursedZones = (p1Inside, p2Inside) switch
        {
            (true, true) => totalFragments,
            (false, false) => 0,
            _ => Random.Range(1, totalFragments)
        };

        int totalFragmentsOutsideCursedZones =
            totalFragments - totalFragmentsInCursedZones;

        List<ItemType> items = new();
        List<int> amounts = new();
        
        List<Vector2Int> positions = new();
        List<Vector2Int> normalZones = new();

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2Int pos = new(x, y);

                if (!generatedZones.Contains(pos))
                    normalZones.Add(pos);
            }
        }

        for (int i = normalZones.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (normalZones[i], normalZones[j]) = (normalZones[j], normalZones[i]);
        }

        for (int i = generatedZones.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (generatedZones[i], generatedZones[j]) = (generatedZones[j], generatedZones[i]);
        }

        bool addedTotalFragmentsInCursedZones = AddPurificationFragmentsToChest(generatedZones, totalFragmentsInCursedZones, 
            positions, items, amounts);

        bool addedTotalFragmentsInNormalZones = AddPurificationFragmentsToChest(normalZones, totalFragmentsOutsideCursedZones,
            positions, items, amounts);

        bool success = addedTotalFragmentsInCursedZones &&
               addedTotalFragmentsInNormalZones;

        if (!success)
            return false;

        player2.SyncChestItemsRpc(
            positions.ToArray(),
            items.ToArray(),
            amounts.ToArray()
        );

        return true;
    }

    private bool AddPurificationFragmentsToChest(List<Vector2Int> zones, int totalFragments, 
        List<Vector2Int> positions, List<ItemType> items, List<int> amounts)
    {
        int actualFragments = 0;

        foreach (Vector2Int pos in zones)
        {
            if (actualFragments >= totalFragments)
                break;

            ZoneData zone = map[pos.y, pos.x];

            if (!zone.chestInventory.HasSpaceFor(ItemType.PurificationFragment))
                continue;

            int remaining = totalFragments - actualFragments;

            int amount = Random.Range(1, Mathf.Min(remaining, 2) + 1);

            if (!zone.chestInventory.Add(ItemType.PurificationFragment, amount))
                continue;

            positions.Add(pos);
            items.Add(ItemType.PurificationFragment);
            amounts.Add(amount);

            actualFragments += amount;
        }

        return actualFragments == totalFragments;
    }

    private List<Vector2Int> GenerateCurseShape(Vector2Int start)
    {
        List<Vector2Int> zones = new() { start };

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (zones.Count < zonesPerCurse)
        {
            List<Vector2Int> candidates = new();

            foreach (Vector2Int zone in zones)
            {
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int next = zone + dir;

                    if (next.x < 0 || next.x >= MapManager.mapWidth ||
                        next.y < 0 || next.y >= MapManager.mapHeight ||
                        zones.Contains(next) ||
                        candidates.Contains(next))
                        continue;

                    candidates.Add(next);
                }
            }

            if (candidates.Count == 0)
                break;

            zones.Add(candidates[Random.Range(0, candidates.Count)]);
        }

        return zones;
    }

    [Rpc(SendTo.Everyone)]
    private void ApplyCurseRpc(Vector2Int[] zones)
    {
        cursedZones.Clear();
        cursedZones.UnionWith(zones);

        Direction[] directions =
        {
            Direction.North,
            Direction.South,
            Direction.East,
            Direction.West
        };

        foreach (Vector2Int pos in zones)
        {
            ZoneData actualZone = map[pos.y, pos.x];

            foreach (Direction dir in directions)
            {
                Vector2Int next = pos + GetDirectionOffset(dir);

                if (next.x < 0 || next.x >= mapWidth ||
                    next.y < 0 || next.y >= mapHeight)
                    continue;

                if (cursedZones.Contains(next))
                    continue;

                ZoneData nextZone = map[next.y, next.x];

                actualZone.blockedDirections.Add(dir);

                nextZone.blockedDirections.Add(GetOppositeDirection(dir));

                Debug.Log($"Blocked: (Y: {pos.y + 1}, X: {pos.x + 1}) {dir} <-> (Y: {next.y + 1}, X: {next.x + 1}) {GetOppositeDirection(dir)}");
            }
        }

        UpdatePlayerCurrentZone();
    }

    [Rpc(SendTo.Everyone)]
    private void ClearCursedZonesRpc()
    {
        cursedZones.Clear();

        foreach (var zone in map)
            zone.blockedDirections.Clear();

        if (IsServer)
        {
            isCurseActive = false;
            State.Value = OwlState.Resting;
            time = moveInterval;
        }

        UpdatePlayerCurrentZone();
        RemoveFragments();
    }

    private void UpdatePlayerCurrentZone()
    {
        Player player = MapManager.Instance.GetLocalPlayer();

        if (player == null || player.currentZone == null)
            return;

        if (player.currentZone.TryGetComponent(out Zone zone))
            zone.Setup(player.NetworkMapPos.Value, map);
    }

    private Vector2Int GetDirectionOffset(Direction direction)
    {
        return direction switch
        {
            Direction.North => Vector2Int.up,
            Direction.South => Vector2Int.down,
            Direction.East => Vector2Int.right,
            Direction.West => Vector2Int.left,
            _ => Vector2Int.zero
        };
    }

    private Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => Direction.None
        };
    }
}

public enum OwlState
{
    Resting,
    WaitingForPurification,
}