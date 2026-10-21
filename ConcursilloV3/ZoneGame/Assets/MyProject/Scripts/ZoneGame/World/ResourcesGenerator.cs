using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

[RequireComponent(typeof(RecoverySystemUI))]
public class ResourcesGenerator : MonoBehaviour
{
    public static ResourcesGenerator Instance { get; private set; }

    public RecoverySystemUI recoverySystemUI;

    private ZoneData[,] map;
    
    private int mapHeight;
    private int mapWidth;
    private float recoveryTimer;
    private const float recoveryInterval = 20f;

    public bool canActivateRecoveryTimer = false;

    List<Vector2Int> momPart = new();
    private readonly List<Vector2Int> allMap = new();

    private Player localPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        recoverySystemUI = GetComponent<RecoverySystemUI>();
    }

    private void Update()
    {
        if (!canActivateRecoveryTimer || localPlayer == null || !localPlayer.IsServer)
            return;

        recoveryTimer += Time.deltaTime;

        if (recoveryTimer >= recoveryInterval)
        {
            recoveryTimer = 0f;
            CheckResources();
        }
    }

    public void Init()
    {
        allMap.Clear();
        momPart.Clear();

        canActivateRecoveryTimer = true;

        localPlayer = MapManager.Instance.GetLocalPlayer();

        const int extraFlowers = 0;
        const int extraBranches = 10;
        const int totalBranchesToCraft = CraftingSystem.branchesForLog + extraBranches;
        const int totalFlowersToCraft = CraftingSystem.flowersForBouquet * CraftingSystem.BouquetsToWin + extraFlowers;

        mapHeight = MapManager.mapHeight;
        mapWidth = MapManager.mapWidth;

        map = MapManager.Instance.map;

        momPart = TilesBeforeRiver(MapManager.Instance.startPos);

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                if (map[i, j].canHaveChest)
                    allMap.Add(new Vector2Int(j, i));
            }
        }
       
        SetInitialItems(ChestItems.Branch, momPart, totalBranchesToCraft);
        //Debug.Log("Items después de añadir ramas");
        //PrintItems();
        
        SetInitialItems(ChestItems.Flower, allMap, totalFlowersToCraft);
        //Debug.Log("Items después de añadir flores");
        //PrintItems();
        
        DistributeAllItems(allMap);

        //Debug.Log("Items después de añadir items aleatorios por el mapa entero");
        //PrintItems();

        //PrintEmptyZones();
    }

    private void CheckResources()
    {
        int bouquetsInTotal = SearchForItem(ItemType.Bouquet, allMap);
        int flowersInTotal = SearchForItem(ItemType.Flower, allMap) + (bouquetsInTotal * CraftingSystem.flowersForBouquet);

        int logsInTotal = SearchForItem(ItemType.Log, momPart, false);
        int branchesInTotal = SearchForItem(ItemType.Branch, momPart, false) + (logsInTotal * CraftingSystem.branchesForLog);

        int flowersToGenerate = FlowersToGenerate(flowersInTotal);
        int branchesToGenerate = BranchesToGenerate(branchesInTotal);

        if (flowersToGenerate == 0 && branchesToGenerate == 0)
            return;

        List<ItemType> items = new();
        List<int> amounts = new();

        if (flowersToGenerate > 0)
        {
            int flowersGenerated = SetInitialItems(
                ChestItems.Flower, allMap, flowersToGenerate);

            items.Add(ItemType.Flower);
            amounts.Add(flowersGenerated);
        }

        if (branchesToGenerate > 0)
        {
            int branchesGenerated = SetInitialItems(
                ChestItems.Branch, momPart, branchesToGenerate);

            items.Add(ItemType.Branch);
            amounts.Add(branchesGenerated);
        }

        localPlayer.ShowRecoverySystemPanelInfoClientRpc(items.ToArray(), amounts.ToArray());
    }

    private int FlowersToGenerate(int flowersInTotal)
    {
        int necessaryFlowers = CraftingSystem.flowersForBouquet * CraftingSystem.BouquetsToWin;

        return flowersInTotal >= necessaryFlowers ? 0 : necessaryFlowers - flowersInTotal;
    }

    private int BranchesToGenerate(int branchesInTotal)
    {
        int necessaryBranches =
            CraftingSystem.branchesForLog * CraftingSystem.logsForBridge;

        if (branchesInTotal >= necessaryBranches)
            return 0;

        int logsInWolfDen = map.Cast<ZoneData>()
            .Where(zone => zone.type == ZoneType.WolfDen)
            .Sum(zone => zone.chestInventory.GetItemAmount(ItemType.Log));

        int branchesInWolfDen = map.Cast<ZoneData>()
            .Where(zone => zone.type == ZoneType.WolfDen)
            .Sum(zone => zone.chestInventory.GetItemAmount(ItemType.Branch))
            + logsInWolfDen * CraftingSystem.branchesForLog;

        bool wolfDenIsInMomPart =
            momPart.Contains(MapManager.Instance.wolfDenPos);

        bool hasRiverBridged =
            map.Cast<ZoneData>().Any(zone => zone.riverBridged);

        if (wolfDenIsInMomPart)
        {
            if (branchesInTotal + branchesInWolfDen >= necessaryBranches)
                return 0;

            return necessaryBranches - branchesInTotal - branchesInWolfDen;
        }

        if (hasRiverBridged)
            return 0;

        return necessaryBranches - branchesInTotal;
    }

    private int SearchForItem(ItemType itemType, List<Vector2Int> mapPart, bool checkWolfDen = true)
    {
        int itemCount = 0;

        foreach (Vector2Int pos in mapPart)
        {
            ZoneData zone = map[pos.y, pos.x];

            if (zone.type == ZoneType.WolfDen && !checkWolfDen)
                continue;

            foreach (var item in zone.chestInventory.items)
            {
                if (item.Key == itemType)
                    itemCount += item.Value;
            }
        }

        Player[] players = new[] { MapManager.Instance.Player1, MapManager.Instance.Player2 };

        foreach (Player player in players)
        {
            foreach (var item in player.inventory.items)
            {
                if (item.Key == itemType)
                    itemCount += item.Value;
            }
        }

        return itemCount;
    }

    private void DistributeAllItems(List<Vector2Int> allMap)
    {
        foreach (Vector2Int position in allMap)
        {
            ZoneData zone = map[position.y, position.x];

            int probOfItem = Random.Range(0, 100);

            ChestItems item = zone.ItemProbInChest(probOfItem);

            if (item == ChestItems.None)
                continue;

            int amount = Random.Range(1, MaxItemCountPerChest(item) + 1);

            zone.chestInventory.AddItem((ItemType)item, amount);
        }
    }

    private int SetInitialItems(
        ChestItems item,
        List<Vector2Int> mapPart,
        int totalItemsToCraft)
    {
        int itemCount = 0;
        List<Vector2Int> availablePositions = new(mapPart);

        while (itemCount < totalItemsToCraft)
        {
            if (availablePositions.Count == 0)
            {
                availablePositions = new List<Vector2Int>(mapPart);
            }

            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector2Int position = availablePositions[randomIndex];
            availablePositions.RemoveAt(randomIndex);

            int maxAmount = MaxItemCountPerChest(item);

            if (maxAmount <= 0)
                continue;

            int amount = Random.Range(1, maxAmount + 1);

            if (map[position.y, position.x].chestInventory.AddItem(
                    (ItemType)item, amount))
            {
                itemCount += amount;
            }
        }

        return itemCount;
    }

    private int MaxItemCountPerChest(ChestItems item)
    {
        return item switch
        {
            ChestItems.Flower => 2,
            ChestItems.Branch => 2,
            ChestItems.Coin => 3,
            _ => 0
        };
    }

    private void PrintEmptyZones()
    {
        Debug.Log("Zonas sin items");
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                ZoneData zone = map[y, x];
                if (zone.chestInventory.items.Count == 0)
                    Debug.Log($"{zone.name} ({y + 1}, {x + 1})\n" +
                        $"Puede tener items: {zone.canHaveChest}\n");
            }
        }
    }

    private void PrintItems()
    {
        int flowers = 0;
        int branches = 0;
        int coins = 0;

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                Inventory inventory = map[i, j].chestInventory;

                flowers += inventory.GetItemAmount(ItemType.Flower);
                branches += inventory.GetItemAmount(ItemType.Branch);
                coins += inventory.GetItemAmount(ItemType.Coin);
            }
        }

        Debug.Log($"Total flowers: {flowers}");
        Debug.Log($"Total branches: {branches}");
        Debug.Log($"Total coins: {coins}");
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
            if (map[current.y, current.x].canHaveChest)
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
    Coin,
}
