using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class River : MonoBehaviour
{
    [SerializeField] private GameObject[] bridges;
    [SerializeField] private GameObject[] riverInteractors;
    public Vector2Int mapPosition;
    private ZoneData[,] map;
    void Start()
    {
        map = MapManager.Instance.map;
        SwapGameObjectStatus(map[mapPosition.y, mapPosition.x].riverBridged, bridges);
        SwapGameObjectStatus(!map[mapPosition.y, mapPosition.x].riverBridged, riverInteractors);
    }

    public bool Interact(Player player)
    {
        if (map[mapPosition.y, mapPosition.x].riverBridged)
            return false;

        if (!player.inventory.HasItem(ItemType.Log))
            return false;

        SwapGameObjectStatus(false, riverInteractors);
        SwapGameObjectStatus(true, bridges);

        map[mapPosition.y, mapPosition.x].riverBridged = true;

        return true;
    }

    public void SwapGameObjectStatus(bool status, GameObject[] objects)
    {
        foreach (var item in objects)
        {
            item.SetActive(status);
        }
    }

    public void SwapGameObjectStatusNetworking()
    {
        SwapGameObjectStatus(false, riverInteractors);
        SwapGameObjectStatus(true, bridges);
    }
}
