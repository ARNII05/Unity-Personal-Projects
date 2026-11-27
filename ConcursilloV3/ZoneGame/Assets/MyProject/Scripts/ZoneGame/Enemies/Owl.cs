using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Owl : NetworkBehaviour
{
    private bool readyToMove = false;
    private const float minCurseInterval = 75f;
    private const float maxCurseInterval = 105f;

    private int mapWidth;
    private int mapHeight;

    private float time = 0;

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
            ResetCurseTimer();
        }
    }
    
    private void ResetCurseTimer()
    {
        time = Random.Range(minCurseInterval, maxCurseInterval);
    }

    private void Update()
    {
        if (!IsServer || !readyToMove || isCurseActive || isPreparingCurse)
            return;

        time -= Time.deltaTime;

        if (time <= 0)
        {
            OrganizeCursedZones();
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

        const int maxAttempts = 20;
        bool success = false;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2Int startPos = GetCurseStartPosition();
            List<Vector2Int> generatedZones = GenerateCurseShape(startPos);

            if (generatedZones.Count != zonesPerCurse)
                continue;

            if (!DistributePurificationFragments(generatedZones))
            {
                RemoveFragments();
                continue;
            }

            isCurseActive = true;
            State.Value = OwlState.WaitingForPurification;

            ApplyCurseRpc(generatedZones.ToArray());

            success = true;
            break;
        }

        if (!success)
        {
            Debug.LogWarning("Owl could not find a valid curse region");
            ResetCurseTimer();
        }

        isPreparingCurse = false;
    }

    private Vector2Int GetCurseStartPosition()
    {
        List<(Vector2Int pos, int weight)> candidates = new();

        Vector2Int p1Pos = player1.NetworkMapPos.Value;
        Vector2Int p2Pos = player2.NetworkMapPos.Value;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2Int pos = new(x, y);

                int distance = Mathf.Min(
                    Mathf.Abs(pos.x - p1Pos.x) + Mathf.Abs(pos.y - p1Pos.y),
                    Mathf.Abs(pos.x - p2Pos.x) + Mathf.Abs(pos.y - p2Pos.y)
                );

                int weight = distance switch
                {
                    0 => 15,
                    1 => 10,
                    2 => 6,
                    _ => 1
                };

                candidates.Add((pos, weight));
            }
        }

        int totalWeight = candidates.Sum(c => c.weight);
        int random = Random.Range(0, totalWeight);

        foreach (var (pos, weight) in candidates)
        {
            random -= weight;

            if (random < 0)
                return pos;
        }

        return p1Pos;
    }

    private void RemoveFragments()
    {
        foreach (var zone in map)
        {
            int fragmentsAmount = zone.chestInventory.GetAmount(ItemType.PurificationFragment);

            if (fragmentsAmount > 0)
                zone.chestInventory.Remove(ItemType.PurificationFragment, fragmentsAmount);
        }

        if (!IsServer)
            return;

        foreach (Player player in new[] { player1, player2 })
        {
            int amount = player.inventory.GetAmount(ItemType.PurificationFragment);

            if (amount <= 0)
                continue;

            if (player.inventory.Remove(ItemType.PurificationFragment, amount))
                player.SetInventory();
        }
    }

    private bool DistributePurificationFragments(List<Vector2Int> generatedZones)
    {
        (int inside, int outside) = GetFragmentsDistribution(generatedZones);

        List<Vector2Int> normalZones = GetValidNormalZones(generatedZones);

        ShuffleZones(normalZones);
        ShuffleZones(generatedZones);

        List<Vector2Int> positions = new();
        List<ItemType> items = new();
        List<int> amounts = new();

        bool insideSuccess = AddPurificationFragmentsToChest(
            generatedZones, inside, positions, items, amounts);

        bool outsideSuccess = AddPurificationFragmentsToChest(
            normalZones, outside, positions, items, amounts);

        if (!insideSuccess || !outsideSuccess)
            return false;

        SyncPurificationFragments(positions, items, amounts);

        return true;
    }

    private void ShuffleZones(List<Vector2Int> zones)
    {
        for (int i = zones.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            (zones[i], zones[j]) = (zones[j], zones[i]);
        }
    }

    private (int inside, int outside) GetFragmentsDistribution(
        List<Vector2Int> generatedZones)
    {
        bool p1Inside = generatedZones.Contains(player1.NetworkMapPos.Value);
        bool p2Inside = generatedZones.Contains(player2.NetworkMapPos.Value);

        int totalFragments =
            CraftingSystem.purificationFragmentsForPurificationAmulet;

        int inside = (p1Inside, p2Inside) switch
        {
            (true, true) => totalFragments,
            (false, false) => 0,
            _ => Random.Range(1, totalFragments)
        };

        return (inside, totalFragments - inside);
    }

    private List<Vector2Int> GetValidNormalZones(
        List<Vector2Int> generatedZones)
    {
        bool builderIsInside =
            (player1.Role.Value == PlayerRole.Builder &&
             generatedZones.Contains(player1.NetworkMapPos.Value)) ||
            (player2.Role.Value == PlayerRole.Builder &&
             generatedZones.Contains(player2.NetworkMapPos.Value));

        bool needsMomPartRestriction =
            builderIsInside &&
            !map.Cast<ZoneData>().Any(zone => zone.riverBridged);

        List<Vector2Int> normalZones = new();

        List<Vector2Int> momPart = needsMomPartRestriction
            ? GetTilesInMomPart(MapManager.Instance.startPos)
            : null;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2Int pos = new(x, y);

                if (generatedZones.Contains(pos))
                    continue;

                if (needsMomPartRestriction && !momPart.Contains(pos))
                    continue;

                normalZones.Add(pos);
            }
        }

        return normalZones;
    }

    private void SyncPurificationFragments(
        List<Vector2Int> positions,
        List<ItemType> items,
        List<int> amounts)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int pos = positions[i];

            Debug.Log(
                $"Added {amounts[i]} purification fragments in " +
                $"{map[pos.y, pos.x]} (Y: {pos.y + 1}, X: {pos.x + 1})"
            );
        }

        player2.SyncChestItemsRpc(
            positions.ToArray(),
            items.ToArray(),
            amounts.ToArray()
        );
    }

    private List<Vector2Int> GetTilesInMomPart(Vector2Int position)
    {
        List<Vector2Int> tilesInZone = new();
        bool[,] visited = new bool[mapHeight, mapWidth];
        Queue<Vector2Int> queue = new();

        queue.Enqueue(position);
        visited[position.y, position.x] = true;

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

            tilesInZone.Add(current);

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (next.x < 0 || next.x >= mapWidth ||
                    next.y < 0 || next.y >= mapHeight)
                    continue;

                if (visited[next.y, next.x])
                    continue;

                ZoneData nextZone = map[next.y, next.x];

                if (nextZone == null)
                    continue;

                visited[next.y, next.x] = true;

                bool isRiver = nextZone.type == ZoneType.VerticalRiver ||
                               nextZone.type == ZoneType.HorizontalRiver;

                if (isRiver)
                {
                    tilesInZone.Add(next);
                    continue;
                }

                queue.Enqueue(next);
            }
        }

        return tilesInZone;
    }

    private bool AddPurificationFragmentsToChest(List<Vector2Int> zones, int totalFragments, 
        List<Vector2Int> positions, List<ItemType> items, List<int> amounts)
    {
        int actualFragments = 0;
        int attemps = 0;
        const int maxAttemps = 100;

        while (attemps < maxAttemps && 
            actualFragments < totalFragments)
        {
            foreach (Vector2Int pos in zones)
            {
                if (actualFragments >= totalFragments)
                    break;

                ZoneData zone = map[pos.y, pos.x];

                if (zone.type == ZoneType.Swamp)
                    continue;

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

            attemps++;
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
            ResetCurseTimer();
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

    public enum OwlState
    {
        Resting,
        WaitingForPurification,
    }
}