using Unity.Netcode;
using UnityEngine;

public class Owl : NetworkBehaviour
{
    private bool readyToMove = false;
    private const int moveInterval = 20;

    private int mapWidth;
    private int mapHeight;

    private float time = moveInterval;

    private Player player1, player2;

    private ZoneData[,] map;

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
}

public enum OwlState
{
    Resting,
    Cursing,
    WaitingForPurification,
}