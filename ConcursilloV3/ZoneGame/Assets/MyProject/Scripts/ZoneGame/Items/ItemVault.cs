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

    public static bool IsStealable(ItemType itemType)
    {
        return itemType != ItemType.Map && itemType != ItemType.Radar
            && itemType != ItemType.None && itemType != ItemType.AncientRelic &&
            itemType != ItemType.AncestralNecklace;
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

    public static int GetBuffValue(ItemType itemType, BuffApplierType applierType)
    {
        return (itemType, applierType) switch
        {
            (ItemType.AncestralNecklace, BuffApplierType.Wolf) => 15,
            (ItemType.AncestralNecklace, BuffApplierType.Swamp) => 25,
            (ItemType.AgilityPotion, BuffApplierType.Movement) => 10,
            (ItemType.LuckPotion, BuffApplierType.Wolf) => 15,
            _ => 0
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
    AncestralNecklace,
    AgilityPotion,
    LuckPotion,
}

public enum BuffApplierType
{
    None,
    Wolf,
    Swamp,
    Movement,
}