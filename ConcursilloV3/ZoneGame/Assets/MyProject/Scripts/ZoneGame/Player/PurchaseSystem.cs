using System.Collections.Generic;
using UnityEngine;

public class PurchaseSystem : MonoBehaviour
{
    public static PurchaseSystem Instance { get; private set; }

    public const int flowerCost = 2;
    public const int branchCost = 4;
    public const int meatCost = 6;

    public const int maxFlowers = 5;
    public const int maxBranches = 5;
    public const int maxMeats = 5;

    private readonly Dictionary<Vector2Int, VillageStock> villageStocks = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public PurchaseResult CanTradeItem(ItemType itemToPrice, ItemType itemToAdd, ItemType itemToRemove, 
        int actualAmount, TradeMode tradeMode, Player player, Vector2Int pos)
    {
        if (itemToPrice == ItemType.None || itemToRemove == ItemType.None || itemToAdd == ItemType.None || actualAmount <= 0)
            return PurchaseResult.Error;

        if (tradeMode != TradeMode.Buy && tradeMode != TradeMode.Sell)
            return PurchaseResult.Error;

        if (!ItemToPriceIsValid(itemToPrice) || !ItemsSendedAreValid(itemToPrice, itemToAdd, itemToRemove, tradeMode))
            return PurchaseResult.Error;

        int amountToRemove = itemToRemove == ItemType.Coin
            ? GetItemPrice(itemToPrice) * actualAmount
            : actualAmount;

        bool hasEnoughItems =
            player.inventory.GetAmount(itemToRemove) >= amountToRemove;
        bool hasEnoughAmount = GetAmountLeft(pos, itemToPrice, tradeMode) >= actualAmount;
        bool hasSpaceInInventory =
            player.inventory.HasSpaceAfterRemoving(
                itemToRemove,
                amountToRemove,
                itemToAdd
            );

        return (hasSpaceInInventory, hasEnoughItems, hasEnoughAmount) switch
        {
            (false, _, _) => PurchaseResult.InventoryFull,
            (true, false, _) => PurchaseResult.NotEnoughItems,
            (true, true, false) => PurchaseResult.OutOfStock,
            _ => PurchaseResult.Success
        };
    }

    public PurchaseResult TradeItem(ItemType item, ItemType itemToAdd, ItemType itemToRemove, 
        int actualAmount, TradeMode tradeMode, Player player, Vector2Int pos)
    {
        PurchaseResult purchaseResult = CanTradeItem(item, itemToAdd, itemToRemove, 
            actualAmount, tradeMode, player, pos);
        
        if (purchaseResult != PurchaseResult.Success)
            return purchaseResult;

        int amountToRemove = itemToRemove == ItemType.Coin ? GetItemPrice(item) * actualAmount : actualAmount;
        int amountToAdd = itemToAdd == ItemType.Coin ? GetItemPrice(item) * actualAmount : actualAmount;

        if (!player.inventory.Remove(itemToRemove, amountToRemove))
            return PurchaseResult.Error;

        if (!player.inventory.Add(itemToAdd, amountToAdd))
        {
            player.inventory.Add(itemToRemove, amountToRemove);
            return PurchaseResult.Error;
        }

        return PurchaseResult.Success;
    }

    public void UpdateStock(
        Vector2Int position,
        TradeMode tradeMode,
        ItemType item,
        int actualAmount)
    {
        VillageStock village = GetVillageStock(position);

        Dictionary<ItemType, int> stock = tradeMode switch
        {
            TradeMode.Buy => village.BuyStock,
            TradeMode.Sell => village.SellStock,
            _ => null
        };

        if (stock == null || !stock.ContainsKey(item))
            return;

        stock[item] -= actualAmount;
    }

    private bool ItemToPriceIsValid(ItemType itemToPrice)
    {
        return itemToPrice == ItemType.Flower || itemToPrice == ItemType.Branch || itemToPrice == ItemType.Meat;
    }

    private VillageStock GetVillageStock(Vector2Int position)
    {
        if (!villageStocks.TryGetValue(position, out VillageStock stock))
        {
            stock = new VillageStock();
            villageStocks.Add(position, stock);
        }

        return stock;
    }

    private bool ItemsSendedAreValid(ItemType item, ItemType itemToAdd, ItemType itemToRemove, TradeMode tradeMode)
    {
        ItemType expectedToRemove =
            tradeMode == TradeMode.Buy ? ItemType.Coin : item;

        ItemType expectedToAdd =
            tradeMode == TradeMode.Buy ? item : ItemType.Coin;

        if (itemToRemove != expectedToRemove ||
            itemToAdd != expectedToAdd)
            return false;

        return true;
    }

    public int GetItemPrice(ItemType item)
    {
        return item switch
        {
            ItemType.Flower => flowerCost,
            ItemType.Branch => branchCost,
            ItemType.Meat => meatCost,
            _ => 0
        };
    }

    public int GetAmountLeft(
        Vector2Int position,
        ItemType item,
        TradeMode tradeMode)
    {
        VillageStock village = GetVillageStock(position);

        Dictionary<ItemType, int> stock = tradeMode switch
        {
            TradeMode.Buy => village.BuyStock,
            TradeMode.Sell => village.SellStock,
            _ => null
        };

        return stock != null && stock.TryGetValue(item, out int amount)
            ? amount
            : 0;
    }
}
public class VillageStock
{
    public Dictionary<ItemType, int> BuyStock { get; } = new()
    {
        [ItemType.Flower] = PurchaseSystem.maxFlowers,
        [ItemType.Branch] = PurchaseSystem.maxBranches,
        [ItemType.Meat] = PurchaseSystem.maxMeats
    };

    public Dictionary<ItemType, int> SellStock { get; } = new()
    {
        [ItemType.Flower] = PurchaseSystem.maxFlowers,
        [ItemType.Branch] = PurchaseSystem.maxBranches,
        [ItemType.Meat] = PurchaseSystem.maxMeats
    };
}

public enum PurchaseResult
{
    Error,
    Success,
    InvalidAmount,
    NotEnoughItems,
    OutOfStock,
    InventoryFull,
}

public enum TradeMode
{
    Buy,
    Sell,
    None
}