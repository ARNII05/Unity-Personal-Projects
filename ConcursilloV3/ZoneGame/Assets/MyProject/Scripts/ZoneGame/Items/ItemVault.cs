using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemVault
{
    public static Items GenerateItem(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Flower => new Flower(),
            ItemType.Bouquet => new Bouquet(),
            ItemType.Log => new Log(),
            ItemType.Branch => new Branch(),
            ItemType.Meat => new Meat(),
            ItemType.Coin => new Coin(),
            ItemType.Radar => new Radar(),
            ItemType.Map => new Map(),
            _ => null,
        };
    }

    public static bool IsTransferable(ItemType itemType)
    {
        return itemType != ItemType.Map && itemType != ItemType.Radar
            && itemType != ItemType.None;
    }
}
