using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class River : MonoBehaviour
{
    [SerializeField] private GameObject[] bridges;
    [SerializeField] private GameObject[] riverInteractors;
    [SerializeField] private GameObject[] triggers;
    [SerializeField] private RiverType riverType;
    
    public Vector2Int mapPosition;
    private ZoneData[,] map;
    public Direction direction;
    
    void Start()
    {
        map = MapManager.Instance.map;
        SwapGameObjectStatus(map[mapPosition.y, mapPosition.x].riverBridged, bridges);
        SwapGameObjectStatus(!map[mapPosition.y, mapPosition.x].riverBridged, riverInteractors);
        SwapGameObjectStatus(!map[mapPosition.y, mapPosition.x].riverBridged, triggers);
        InitHorizontalRiver();
    }

    public void SetInfo(Vector2Int mapPosition, Direction direction)
    {
        this.mapPosition = mapPosition;
        this.direction = direction;
    }

    private void InitHorizontalRiver()
    {
        if (riverType != RiverType.Horizontal)
            return;

        if (map[mapPosition.y, mapPosition.x].riverBridged)
            return;

        riverInteractors[0].SetActive(!(direction == Direction.North));
        riverInteractors[1].SetActive(!(direction == Direction.South));

        bridges[0].SetActive(direction == Direction.North);
        bridges[1].SetActive(direction == Direction.South);
    }

    public bool Interact(Player player)
    {
        if (map[mapPosition.y, mapPosition.x].riverBridged)
            return false;

        if (player.inventory.GetItemAmount(ItemType.Log) <= 0)
            return false;

        SwapGameObjectStatus(false, riverInteractors);
        SwapGameObjectStatus(false, triggers);
        SwapGameObjectStatus(true, bridges);
        
        map[mapPosition.y, mapPosition.x].riverBridged = true;

        return true;
    }

    private void SwapGameObjectStatus(bool status, GameObject[] objects)
    {
        foreach (var item in objects)
        {
            item.SetActive(status);
        }
    }

    public void SwapGameObjectStatusNetworking()
    {
        SwapGameObjectStatus(false, riverInteractors);
        SwapGameObjectStatus(false, triggers);
        SwapGameObjectStatus(true, bridges);
    }
}

public enum RiverType
{
    Horizontal,
    Vertical
}