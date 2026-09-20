using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private Dictionary<ItemType, int> items = new();
    private const int maxCapacity = 5;

    public bool AddItem(ItemType itemType, int amount)
    {
        if (items.ContainsKey(itemType))
        {
            items[itemType] += amount;
            return true;
        }

        if (items.Count >= maxCapacity)
        {
            return false;
        }

        items[itemType] = amount;
        return true;
    }

    public int GetItemAmount(ItemType itemType)
    {
        if (items.ContainsKey(itemType))
        {
            return items[itemType];
        }
        
        return 0;
    }

    public bool RemoveItem(ItemType itemType, int amount)
    {
        if (!items.ContainsKey(itemType))
        {
            return false;
        }

        items[itemType] -= amount;
        if (items[itemType] <= 0)
        {
            items.Remove(itemType);
        }
        
        return true;
    }

    public void PrintInventory()
    {
        foreach (var item in items)
        {
            UIManager.Instance.WriteOnLogs($"Item: {item.Key}, Amount: {item.Value}\n");
        }
    }
}