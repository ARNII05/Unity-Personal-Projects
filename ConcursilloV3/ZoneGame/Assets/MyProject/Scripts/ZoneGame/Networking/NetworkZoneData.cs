using System.Linq;
using Unity.Netcode;

public struct NetworkZoneData : INetworkSerializable
{
    public ZoneType type;
    public Direction firstRiverDirection;
    public bool riverBridged;
    public bool chestOpened;

    public ItemType[] chestItemTypes;
    public int[] chestAmounts;

    public NetworkZoneData(ZoneData zone)
    {
        type = zone.type;
        riverBridged = zone.riverBridged;
        firstRiverDirection = zone.firstRiverDirection;
        chestOpened = zone.chestOpened;

        chestItemTypes = zone.chestInventory.items.Keys.ToArray();
        chestAmounts = zone.chestInventory.items.Values.ToArray();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref riverBridged);
        serializer.SerializeValue(ref chestOpened);
        serializer.SerializeValue(ref chestItemTypes);
        serializer.SerializeValue(ref chestAmounts);
    }
}