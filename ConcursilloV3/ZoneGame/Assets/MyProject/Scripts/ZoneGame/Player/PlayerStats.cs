using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats
{
    private readonly Dictionary<StatsType, int> stats = new();

    public void SaveStats(StatsType statsType, int amount)
    {
        stats[statsType] = stats.GetValueOrDefault(statsType) + amount;
    }

    public PlayerStatsData GetStatsData()
    {
        return new PlayerStatsData
        {
            ItemsCollected = stats.GetValueOrDefault(StatsType.ItemsPicked),
            ItemsCrafted = stats.GetValueOrDefault(StatsType.ObjectsCrafted),
            ZonesExplored = stats.GetValueOrDefault(StatsType.DiscoveredZones),
            TimesRobbed = stats.GetValueOrDefault(StatsType.TimesStolenByWolf),
            LostObjectsCount = stats.GetValueOrDefault(StatsType.LostObjectsCount)
        };
    }
}

public struct PlayerStatsData : INetworkSerializeByMemcpy
{
    public int ItemsCollected;
    public int ItemsCrafted;
    public int ZonesExplored;
    public int TimesRobbed;
    public int LostObjectsCount;

    public readonly int[] GetStatValues()
    {
        return new int[]
        {
            ItemsCollected,
            ZonesExplored,
            TimesRobbed,
            LostObjectsCount,
            ItemsCrafted
        };
    }
}

public enum StatsType
{
    ItemsPicked,
    ObjectsCrafted,
    DiscoveredZones,
    TimesStolenByWolf,
    LostObjectsCount,
}