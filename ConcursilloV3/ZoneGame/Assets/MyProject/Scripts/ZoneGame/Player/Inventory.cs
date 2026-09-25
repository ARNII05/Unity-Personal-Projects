using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory
{
    public Dictionary<ItemType, int> items = new();
    public event Action OnInventoryChanged;
    private const int maxCapacity = 7;

    public bool AddItem(ItemType itemType, int amount)
    {
        if (items.ContainsKey(itemType))
        {
            items[itemType] += amount;
            return true;
        }

        if (items.Count >= maxCapacity)
            return false;

        items[itemType] = amount;

        return true;
    }

    public void SetItems(ItemType[] itemTypes, int[] amounts)
    {
        items.Clear();

        for (int i = 0; i < itemTypes.Length; i++)
        {
            items[itemTypes[i]] = amounts[i];
        }

        OnInventoryChanged?.Invoke();
    }

    public bool HasItem(ItemType itemType)
    {
        return items.TryGetValue(itemType, out int amount) &&
               amount > 0;
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

        OnInventoryChanged?.Invoke();
        return true;
    }

    public void PrintInventory()
    {
        if (items.Count == 0)
        {
            return;
        }
        
        foreach (var item in items)
        {
            Debug.Log($"Item: {item.Key}, Amount: {item.Value}\n");
        }
    }
}