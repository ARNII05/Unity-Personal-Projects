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
            && itemType != ItemType.None && itemType != ItemType.AncientRelic;
    }

    public static bool IsBuff(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.AgilityPotion => true,
            ItemType.LuckPotion => true,
            _ => false
        };
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
    Map,
    AncientRelic,
    AmuletOfTheAncients,
    AgilityPotion,
    LuckPotion,
}