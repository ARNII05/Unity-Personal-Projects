using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory
{
    public Dictionary<ItemType, int> items = new();
    public event Action OnInventoryChanged;
    public const int maxCapacity = 4;

    public bool AddItem(ItemType itemType, int amount)
    {
        if (items.ContainsKey(itemType))
        {
            items[itemType] += amount;
            OnInventoryChanged?.Invoke();
            return true;
        }

        if (items.Count >= maxCapacity)
            return false;

        items[itemType] = amount;
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
}