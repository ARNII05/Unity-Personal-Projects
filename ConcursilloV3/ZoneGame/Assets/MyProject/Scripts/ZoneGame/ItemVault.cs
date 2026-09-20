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
            ItemType.Log => new Log(),
            ItemType.Meat => new Meat(),
            ItemType.Coin => new Coin(),
            ItemType.Radar => new Radar(),
            ItemType.Map => new Map(),
            _ => null,
        };
    }
}
