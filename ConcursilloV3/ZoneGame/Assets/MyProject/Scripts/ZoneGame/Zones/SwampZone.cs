using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwampZone : MonoBehaviour
{
    private const int maxCountToSteal = 3;

    private const int maxItemsToSteal = 1;

    private const int probToRecoverItem = 50;

    private ZoneData[,] map;

    bool hasEnteredOnSwamp = false;

    public void Init(Vector2Int position, Player player)
    {
        map = MapManager.Instance.map;

        if (player.IsServer && !hasEnteredOnSwamp)
        {
            hasEnteredOnSwamp = true;
            ChooseItemToSteal(player, position);
        }
    }
    
    private void ChooseItemToSteal(Player player, Vector2Int swampPos)
    {
        ItemsStolenResult itemsStolen = StealingItems(maxItemsToSteal, swampPos, player);

        player.SetInventory();

        StartCoroutine(SwampNetwork.Instance.ShowUIToUserCoroutine(itemsStolen.items.ToArray(), 
            itemsStolen.amount.ToArray(), player));

        SwampNetwork.Instance.TryToGetStolenItems(probToRecoverItem, swampPos, player.OwnerClientId, itemsStolen.items.Count <= 0);
    }

    private ItemsStolenResult StealingItems(
        int itemsToSteal,
        Vector2Int swampPos,
        Player player)
    {
        List<ItemType> stolenItems = new();
        List<int> amount = new();
        HashSet<ItemType> forbbidenItems = new() { ItemType.None, ItemType.Map, ItemType.Radar};
        Swamp swamp = (Swamp)map[swampPos.y, swampPos.x];

        for (int i = 0; i < itemsToSteal; i++)
        {
            ItemType[] validItems = player.inventory.items.Keys
                .Where(item => !forbbidenItems.Contains(item))
                .ToArray();

            if (validItems.Length == 0)
                break;

            int itemIndex = Random.Range(0, validItems.Length);
            ItemType item = validItems[itemIndex];

            int availableAmount = player.inventory.GetAmount(item);

            int amountToSteal = Random.Range(
                1,
                Mathf.Min(MaxAmountOf(item), availableAmount) + 1
            );

            if (!player.inventory.Remove(item, amountToSteal))
                continue;

            swamp.internalInventory[item] = swamp.internalInventory.GetValueOrDefault(item) + amountToSteal;
            forbbidenItems.Add(item);
            stolenItems.Add(item);
            amount.Add(amountToSteal);
        }

        return new ItemsStolenResult(stolenItems, amount);
    }

    private int MaxAmountOf(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Bouquet or ItemType.Log => 1,
            _ => maxCountToSteal
        };
    }

    private struct ItemsStolenResult
    {
        public List<ItemType> items;
        public List<int> amount;

        public ItemsStolenResult(List<ItemType> items, List<int> amount)
        {
            this.items = items;
            this.amount = amount;
        }
    }
}