using System.Linq;
using Unity.Netcode;

public struct NetworkZoneData : INetworkSerializable
{
    public ZoneType type;
    public bool chestOpened;
    public bool riverBridged;

    public ItemType[] chestItemTypes;
    public int[] chestAmounts;

    public NetworkZoneData(ZoneData zone)
    {
        type = zone.type;
        chestOpened = zone.chestOpened;
        riverBridged = zone.riverBridged;

        chestItemTypes = zone.chestInventory.items.Keys.ToArray();
        chestAmounts = zone.chestInventory.items.Values.ToArray();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref chestOpened);
        serializer.SerializeValue(ref riverBridged);
        serializer.SerializeValue(ref chestItemTypes);
        serializer.SerializeValue(ref chestAmounts);
    }
}