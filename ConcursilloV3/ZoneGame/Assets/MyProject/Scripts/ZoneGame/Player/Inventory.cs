using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory
{
    public Dictionary<ItemType, int> items = new();
    public List<ItemType> inventoryOrder = new();
    public event Action OnInventoryChanged;
    public const int maxCapacity = 5;
    
    public void SetContents(ItemType[] itemTypes, int[] amounts, ItemType[] order)
    {
        items.Clear();
        inventoryOrder.Clear();

        for (int i = 0; i < itemTypes.Length; i++)
            items[itemTypes[i]] = amounts[i];

        inventoryOrder.AddRange(order);

        OnInventoryChanged?.Invoke();
    }

    public bool Add(ItemType itemType, int amount)
    {
        if (amount <= 0 || itemType == ItemType.None)
            return false;
        
        if (items.Count >= maxCapacity && items.GetValueOrDefault(itemType) == 0)
            return false;

        bool isNewItem = !items.ContainsKey(itemType);

        items[itemType] = items.GetValueOrDefault(itemType) + amount;

        if (isNewItem)
            inventoryOrder.Add(itemType);

        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool HasSpaceFor(ItemType itemType)
    {
        return items.ContainsKey(itemType) || items.Count < maxCapacity;
    }

    public bool HasSpaceAfterRemoving(
        ItemType itemToRemove,
        int amountToRemove,
        ItemType itemToAdd)
    {
        bool alreadyHasItem = items.ContainsKey(itemToAdd);

        if (alreadyHasItem)
            return true;

        int remainingAmount = GetAmount(itemToRemove) - amountToRemove;

        bool freesSlot = remainingAmount <= 0;

        return items.Count - (freesSlot ? 1 : 0) < maxCapacity;
    }

    public int GetAmount(ItemType itemType)
    {
        return items.TryGetValue(itemType, out int amount) ? amount : 0;
    }

    public ItemType GetItemAt(int index)
    {
        if (index < 0 || index >= inventoryOrder.Count)
            return ItemType.None;

        return inventoryOrder[index];
    }

    public bool Remove(ItemType itemType, int amount)
    {
        if (!items.ContainsKey(itemType) || amount <= 0 
            || GetAmount(itemType) < amount)
        {
            return false;
        }

        items[itemType] -= amount;
        if (items[itemType] <= 0)
        {
            items.Remove(itemType);
            inventoryOrder.Remove(itemType);
        }

        OnInventoryChanged?.Invoke();
        return true;
    }
}