using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourcesGenerator : MonoBehaviour
{
    public static ResourcesGenerator Instance { get; private set; }
    
    private ZoneData[,] map;
    
    private int mapHeight;
    private int mapWidth;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        mapHeight = MapManager.mapHeight;
        mapWidth = MapManager.mapWidth;
    }

    public void Init()
    {   
        map = MapManager.Instance.map;

        List<Vector2Int> allMap = new();
        
        List<Vector2Int> momPart = TilesBeforeRiver(MapManager.Instance.startPos);

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                if (map[i, j].ItemProbInChest(100) == ChestItems.None)
                    continue;
                
                allMap.Add(new Vector2Int(j, i));
            }
        }

        int branchesCount = SetInitialItems(ChestItems.Branch, momPart, CraftingSystem.branchesForLog);
        int flowerCount = SetInitialItems(ChestItems.Flower, allMap, CraftingSystem.flowersForBouquet);

        DistributeAllItems(allMap);

        Debug.Log($"Initial branches count in mom part: {branchesCount}");
        Debug.Log($"Initial flowers count in all map: {flowerCount}");

        PrintItems();
    }

    private void DistributeAllItems(List<Vector2Int> allMap)
    {
        foreach (Vector2Int position in allMap)
        {
            ZoneData zone = map[position.y, position.x];

            int probOfItem = Random.Range(0, 101);

            ChestItems item = zone.ItemProbInChest(probOfItem);

            if (item == ChestItems.None)
                continue;

            int amount = Random.Range(1, MaxItemCountPerChest(item) + 1);

            zone.chestInventory.AddItem((ItemType)item, amount);
        }
    }

    private int SetInitialItems(ChestItems item, 
        List<Vector2Int> mapPart, int totalItemsToCraft,
        int itemCount = 0)
    {
        List<Vector2Int> mapPartTmp = new(mapPart);

        while (mapPartTmp.Count > 0 && itemCount < totalItemsToCraft)
        {
            int randomPosInt = Random.Range(0, mapPartTmp.Count);

            Vector2Int randomPos = mapPartTmp[randomPosInt];

            int amount = Random.Range(0, MaxItemCountPerChest(item) + 1);
            itemCount += amount;

            map[randomPos.y, randomPos.x].chestInventory.AddItem((ItemType) item, amount);

            mapPartTmp.RemoveAt(randomPosInt);
        }

        if (itemCount < totalItemsToCraft)
        {
            return SetInitialItems(
                item,
                mapPart,
                totalItemsToCraft,
                itemCount
            );
        }

        return itemCount;
    }

    private int MaxItemCountPerChest(ChestItems item)
    {
        return item switch
        {
            ChestItems.Flower => 3,
            ChestItems.Branch => 2,
            ChestItems.Meat => 1,
            ChestItems.Coin => 2,
            _ => throw new System.NotImplementedException(),
        };
    }

    private void PrintItems()
    {
        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                Inventory inventory = map[i, j].chestInventory;

                string items = "";

                int flowers = inventory.GetItemAmount(ItemType.Flower);
                int branches = inventory.GetItemAmount(ItemType.Branch);
                int meat = inventory.GetItemAmount(ItemType.Meat);
                int coins = inventory.GetItemAmount(ItemType.Coin);

                if (flowers > 0)
                    items += $"Flower x{flowers}, ";

                if (branches > 0)
                    items += $"Branch x{branches}, ";

                if (meat > 0)
                    items += $"Meat x{meat}, ";

                if (coins > 0)
                    items += $"Coin x{coins}, ";

                if (items == "")
                    items = "Empty";

                items = items.TrimEnd(' ', ',');

                Debug.Log(
                    $"Posición ({j}, {i}) [{map[i, j].type}]: {items}"
                );
            }
        }
    }

    private List<Vector2Int> TilesBeforeRiver(Vector2Int position)
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
            if (map[current.y, current.x].type != ZoneType.MomHouse)
            {
                tilesInZone.Add(current);
            }

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (next.x < 0 || next.x >= mapWidth ||
                    next.y < 0 || next.y >= mapHeight)
                    continue;

                if (visited[next.y, next.x])
                    continue;

                if (map[next.y, next.x] != null &&
                    (map[next.y, next.x].type == ZoneType.VerticalRiver || map[next.y, next.x].type == ZoneType.HorizontalRiver))
                    continue;

                visited[next.y, next.x] = true;
                queue.Enqueue(next);
            }
        }

        return tilesInZone;
    }
}

public enum ChestItems
{
    None,
    Flower,
    Branch,
    Meat,
    Coin,
}
