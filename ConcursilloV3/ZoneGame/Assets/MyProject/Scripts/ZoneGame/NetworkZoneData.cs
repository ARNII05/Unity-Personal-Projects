using Unity.Netcode;

public struct NetworkZoneData : INetworkSerializable
{
    public ZoneType type;
    public bool chestOpened;
    public int flowersRemaining;
    public bool riverBridged;

    public NetworkZoneData(ZoneData zone)
    {
        type = zone.type;
        chestOpened = zone.chestOpened;
        flowersRemaining = zone.flowersRemaining;
        riverBridged = zone.riverBridged;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref chestOpened);
        serializer.SerializeValue(ref flowersRemaining);
        serializer.SerializeValue(ref riverBridged);
    }
}