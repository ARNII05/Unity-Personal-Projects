using UnityEngine;
using static UnityEditor.Progress;

public class PurchaseSystem : MonoBehaviour
{
    public static PurchaseSystem Instance { get; private set; }

    public const int flowerCost = 2;
    public const int branchCost = 4;
    public const int meatCost = 3;

    public const int maxFlowers = 5;
    public const int maxBranches = 5;
    public const int maxMeats = 5;

    private int buyingFlowersLeft = maxFlowers;
    private int buyingBranchesLeft = maxBranches;
    private int buyingMeatsLeft = maxMeats;

    private int sellingFlowersLeft = maxFlowers;
    private int sellingBranchesLeft = maxBranches;
    private int sellingMeatsLeft = maxMeats;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public PurchaseResult CanTradeItem(ItemType itemToPrice, ItemType itemToAdd, ItemType itemToRemove, int actualAmount, TradeMode tradeMode, Player player)
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
            player.inventory.GetItemAmount(itemToRemove) >= amountToRemove;
        bool hasEnoughAmount = GetAmountLeft(itemToPrice, tradeMode) >= actualAmount;
        bool hasSpaceInInventory = tradeMode switch
        {
            TradeMode.Buy => player.inventory.HasSpaceForItem(itemToAdd),
            _ => player.inventory.HasSpaceAfterRemovingItem(
            itemToRemove,
            amountToRemove,
            itemToAdd)
        };

        return (hasSpaceInInventory, hasEnoughItems, hasEnoughAmount) switch
        {
            (false, _, _) => PurchaseResult.InventoryFull,
            (true, false, _) => PurchaseResult.NotEnoughItems,
            (true, true, false) => PurchaseResult.OutOfStock,
            _ => PurchaseResult.Success
        };
    }

    public PurchaseResult TradeItem(ItemType item, ItemType itemToAdd, ItemType itemToRemove, int actualAmount, TradeMode tradeMode, Player player)
    {
        PurchaseResult purchaseResult = CanTradeItem(item, itemToAdd, itemToRemove, actualAmount, tradeMode, player);
        
        if (purchaseResult != PurchaseResult.Success)
            return purchaseResult;

        int amountToRemove = itemToRemove == ItemType.Coin ? GetItemPrice(item) * actualAmount : actualAmount;
        int amountToAdd = itemToAdd == ItemType.Coin ? GetItemPrice(item) * actualAmount : actualAmount;

        if (!player.inventory.RemoveItem(itemToRemove, amountToRemove))
            return PurchaseResult.Error;

        if (!player.inventory.AddItem(itemToAdd, amountToAdd))
        {
            player.inventory.AddItem(itemToRemove, amountToRemove);
            return PurchaseResult.Error;
        }

        return PurchaseResult.Success;
    }

    public void UpdateStock(TradeMode tradeMode, ItemType item, int actualAmount)
    {
        if (tradeMode == TradeMode.Buy)
            UpdateBuyStock(item, actualAmount);
        else
            UpdateSellStock(item, actualAmount);
    }

    private void UpdateBuyStock(ItemType item, int actualAmount)
    {
        buyingFlowersLeft = item == ItemType.Flower ? buyingFlowersLeft - actualAmount : buyingFlowersLeft;
        buyingBranchesLeft = item == ItemType.Branch ? buyingBranchesLeft - actualAmount : buyingBranchesLeft;
        buyingMeatsLeft = item == ItemType.Meat ? buyingMeatsLeft - actualAmount : buyingMeatsLeft;
    }

    private void UpdateSellStock(ItemType item, int actualAmount)
    {
        sellingFlowersLeft = item == ItemType.Flower ? sellingFlowersLeft - actualAmount : sellingFlowersLeft;
        sellingBranchesLeft = item == ItemType.Branch ? sellingBranchesLeft - actualAmount : sellingBranchesLeft;
        sellingMeatsLeft = item == ItemType.Meat ? sellingMeatsLeft - actualAmount : sellingMeatsLeft;
    }

    private bool ItemToPriceIsValid(ItemType itemToPrice)
    {
        return itemToPrice == ItemType.Flower || itemToPrice == ItemType.Branch || itemToPrice == ItemType.Meat;
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

    public int GetAmountLeft(ItemType item, TradeMode tradeMode)
    {
        return item switch
        {
            ItemType.Flower => tradeMode == TradeMode.Buy ? buyingFlowersLeft : sellingFlowersLeft,
            ItemType.Branch => tradeMode == TradeMode.Buy ? buyingBranchesLeft : sellingBranchesLeft,
            ItemType.Meat => tradeMode == TradeMode.Buy ? buyingMeatsLeft : sellingMeatsLeft,
            _ => 0
        };
    }
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