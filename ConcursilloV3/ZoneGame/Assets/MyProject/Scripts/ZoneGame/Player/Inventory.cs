using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory
{
    public Dictionary<ItemType, int> items = new();
    public event Action OnInventoryChanged;
    public const int maxCapacity = 5;
    
    public void SetInventory(ItemType[] itemTypes, int[] amounts)
    {
        items.Clear();

        for (int i = 0; i < itemTypes.Length; i++)
            items[itemTypes[i]] = amounts[i];

        OnInventoryChanged?.Invoke();
    }

    public bool AddItem(ItemType itemType, int amount)
    {
        if (items.Count >= maxCapacity && items.GetValueOrDefault(itemType) == 0)
            return false;

        items[itemType] = items.GetValueOrDefault(itemType) + amount;

        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool HasSpaceForItem(ItemType itemType)
    {
        return items.ContainsKey(itemType) || items.Count < maxCapacity;
    }

    public int GetItemAmount(ItemType itemType)
    {
        return items.TryGetValue(itemType, out int amount) ? amount : 0;
    }

    public ItemType GetItemByIndex(int index)
    {
        int i = 0;

        foreach (var item in items)
        {
            if (index == i)
                return item.Key;
            i++;
        }

        return ItemType.None;
    }

    public bool RemoveItem(ItemType itemType, int amount)
    {
        if (!items.ContainsKey(itemType) || amount <= 0 
            || GetItemAmount(itemType) < amount)
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
}