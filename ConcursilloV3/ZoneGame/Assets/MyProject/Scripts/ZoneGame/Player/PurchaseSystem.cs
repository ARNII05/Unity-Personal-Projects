using UnityEngine;

public class PurchaseSystem : MonoBehaviour
{
    public static PurchaseSystem Instance { get; private set; }

    public const int flowerCost = 2;
    public const int branchCost = 4;
    public const int meatCost = 3;

    public const int maxFlowers = 5;
    public const int maxBranches = 5;
    public const int maxMeats = 5;

    private int flowersLeft = maxFlowers;
    private int branchesLeft = maxBranches;
    private int meatsLeft = maxMeats;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public PurchaseResult CanBuyItem(ItemType item, int actualAmount, Player player)
    {
        if (item == ItemType.None || actualAmount <= 0)
            return PurchaseResult.Error;

        bool hasEnoughCoins = player.inventory.GetItemAmount(ItemType.Coin) >= GetItemPrice(item) * actualAmount;
        bool hasEnoughAmount = GetAmountLeft(item) >= actualAmount;
        bool hasSpaceInInventory = player.inventory.HasSpaceForItem(item);

        return (hasSpaceInInventory, hasEnoughCoins, hasEnoughAmount) switch
        {
            (false, _, _) => PurchaseResult.InventoryFull,
            (true, false, _) => PurchaseResult.NotEnoughCoins,
            (true, true, false) => PurchaseResult.OutOfStock,
            _ => PurchaseResult.Success
        };
    }

    public PurchaseResult BuyItem(ItemType item, int actualAmount, Player player)
    {
        PurchaseResult purchaseResult = CanBuyItem(item, actualAmount, player);
        
        if (purchaseResult != PurchaseResult.Success)
            return purchaseResult;

        int coinsToRemove = GetItemPrice(item) * actualAmount;

        if (!player.inventory.RemoveItem(ItemType.Coin, coinsToRemove))
            return PurchaseResult.Error;

        if (!player.inventory.AddItem(item, actualAmount))
        {
            if (!player.inventory.AddItem(ItemType.Coin, coinsToRemove))
                return PurchaseResult.Error;
        }

        return PurchaseResult.Success;
    }

    public void UpdateStock(ItemType item, int actualAmount)
    {
        flowersLeft = item == ItemType.Flower ? flowersLeft - actualAmount : flowersLeft;
        branchesLeft = item == ItemType.Branch ? branchesLeft - actualAmount : branchesLeft;
        meatsLeft = item == ItemType.Meat ? meatsLeft - actualAmount : meatsLeft;
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

    public int GetAmountLeft(ItemType item)
    {
        return item switch
        {
            ItemType.Flower => flowersLeft,
            ItemType.Branch => branchesLeft,
            ItemType.Meat => meatsLeft,
            _ => 0
        };
    }
}

public enum PurchaseResult
{
    Error,
    Success,
    InvalidAmount,
    NotEnoughCoins,
    OutOfStock,
    InventoryFull
}