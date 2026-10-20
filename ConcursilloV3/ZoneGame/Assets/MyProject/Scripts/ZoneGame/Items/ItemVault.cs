using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemVault
{
    private const string baseSpritePath = "Prefabs/ItemsImg/";
    
    public static Sprite GetItemSprite(ItemType itemType)
    {
        return Resources.Load<Sprite>(baseSpritePath + itemType.ToString());
    }

    public static bool IsTransferable(ItemType itemType)
    {
        return itemType != ItemType.Map && itemType != ItemType.Radar
            && itemType != ItemType.None;
    }
}

public enum ItemType
{
    None,
    Flower,
    Branch,
    Coin,
    Meat,
    Bouquet,
    Log,
    Radar,
    Map
}