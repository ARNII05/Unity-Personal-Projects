using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory
{
    public List<InventorySlot> items = new();
    public readonly HashSet<ItemType> activeBuffs = new();
    public event Action OnInventoryChanged;
    public const int maxCapacity = 5;

    public const int maxBuffs = 6;


    public void SetContents(ItemType[] itemTypes, int[] amounts)
    {
        if (itemTypes == null || amounts == null ||
            itemTypes.Length != amounts.Length)
        {
            Debug.LogError("Worng inventory data in Inventory.SetContents");
            return;
        }

        items.Clear();

        for (int i = 0; i < itemTypes.Length; i++)
            items.Add(new InventorySlot(itemTypes[i], amounts[i]));

        OnInventoryChanged?.Invoke();
    }

    public bool Add(ItemType itemType, int amount)
    {
        if (amount <= 0 || itemType == ItemType.None)
            return false;

        InventorySlot slot = items.Find(x => x.Item == itemType);

        if (slot != null)
        {
            slot.Amount += amount;
        }
        else
        {
            if (items.Count >= maxCapacity)
                return false;

            items.Add(new InventorySlot(itemType, amount));
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool Add(ItemType buffType)
    {
        if (buffType == ItemType.None || !ItemVault.IsBuff(buffType))
            return false;

        if (activeBuffs.Count >= maxBuffs)
            return false;

        if (!activeBuffs.Add(buffType))
            return false;

        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool HasSpaceFor(ItemType itemType)
    {
        return items.Exists(x => x.Item == itemType) || items.Count < maxCapacity || ItemVault.IsBuff(itemType);
    }

    public ItemType TryToGet(ItemType itemType, bool isBuff)
    {
        if (isBuff && activeBuffs.Contains(itemType))
            return itemType;

        return items.Exists(x => x.Item == itemType) ? itemType : ItemType.None;
    }

    public bool HasSpaceAfterRemoving(
        ItemType itemToRemove,
        int amountToRemove,
        ItemType itemToAdd)
    {
        bool alreadyHasItem = items.Exists(x => x.Item == itemToAdd);

        if (alreadyHasItem)
            return true;

        int remainingAmount = GetAmount(itemToRemove) - amountToRemove;

        bool freesSlot = remainingAmount <= 0;

        return items.Count - (freesSlot ? 1 : 0) < maxCapacity;
    }

    public int GetAmount(ItemType itemType)
    {
        InventorySlot slot = items.Find(x => x.Item == itemType);

        return slot != null ? slot.Amount : 0;
    }

    public ItemType GetItemAt(int index)
    {
        if (index < 0 || index >= items.Count)
            return ItemType.None;

        return items[index].Item;
    }

    public ItemType GetBuffAt(int index)
    {
        if (index < 0 || index >= activeBuffs.Count)
            return ItemType.None;

        int i = 0;

        foreach (var item in activeBuffs)
        {
            if (i == index)
                return item;
            i++;
        }

        return ItemType.None;
    }

    public bool Remove(ItemType itemType, int amount)
    {
        if (amount <= 0)
            return false;

        InventorySlot slot = items.Find(x => x.Item == itemType);

        if (slot == null || slot.Amount < amount)
            return false;

        slot.Amount -= amount;

        if (slot.Amount <= 0)
            items.Remove(slot);

        OnInventoryChanged?.Invoke();
        return true;
    }
}

public class InventorySlot
{
    public ItemType Item { get; }
    public int Amount { get; set; }

    public InventorySlot(ItemType item, int amount)
    {
        Item = item;
        Amount = amount;
    }
}