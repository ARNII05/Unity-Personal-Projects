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

    public static string GetItemName(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Flower => "Flor",
            ItemType.Branch => "Rama",
            ItemType.Coin => "Moneda",
            ItemType.Meat => "Carne",
            ItemType.Bouquet => "Ramo de Flores",
            ItemType.Log => "Tronco",
            ItemType.Radar => "Radar",
            ItemType.Map => "Mapa",
            ItemType.AncientRelic => "Reliquia Antigua",
            ItemType.AncestralNecklace => "Collar Ancestral",
            ItemType.AgilityPotion => "Poción de Agilidad",
            ItemType.LuckPotion => "Poción de Suerte",
            _ => ""
        };
    }

    public static string GetItemDescription(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Flower => "Se utiliza para fabricar Ramos de Flores.",
            ItemType.Branch => "Se utiliza para fabricar Troncos.",
            ItemType.Coin => "Se utiliza para comprar objetos.",
            ItemType.Meat => "Sirve para distraer al lobo y evitar que te robe otros objetos",
            ItemType.Bouquet => "Un ramo de flores para entregárselo a la abuela.",
            ItemType.Log => "Permite cruzar los ríos.",
            ItemType.Radar => "Detecta el lobo.",
            ItemType.Map => "Muestra las zonas adyacentes a tu posición.",
            ItemType.AncientRelic => "Una reliquia necesaria para abrir el cofre ancestral.",
            ItemType.AncestralNecklace => $"Protege contra el lobo y el pantano: " +
                $"{GetBuffValue(itemType, BuffApplierType.Wolf)} y {GetBuffValue(itemType, BuffApplierType.Swamp)}%, respectivamente.",
            ItemType.AgilityPotion => $"Aumenta tu velocidad de movimiento un {GetBuffValue(itemType, BuffApplierType.Movement)}%.",
            ItemType.LuckPotion => $"Otorga un {GetBuffValue(itemType, BuffApplierType.Wolf)}% de probabilidad de evitar el robo del lobo al ser capturado.",
            _ => ""
        };
    }

    public static string GetItemType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Flower => "Recurso de fabricación",
            ItemType.Branch => "Recurso de fabricación",
            ItemType.Coin => "Objeto para venta",
            ItemType.Meat => "Consumible",
            ItemType.Bouquet => "Objeto de misión",
            ItemType.Log => "Recurso de fabricación",
            ItemType.Radar => "Objeto de ayuda",
            ItemType.Map => "Objeto de ayuda",
            ItemType.AncientRelic => "Objeto de misión",
            ItemType.AncestralNecklace => "Objeto de protección",
            ItemType.AgilityPotion => "Consumible",
            ItemType.LuckPotion => "Consumible",
            _ => ""
        };
    }

    public static int MaxAmountOf(ItemType itemType, int maxCountToSteal)
    {
        return itemType switch
        {
            ItemType.Bouquet or ItemType.Log => 1,
            _ => maxCountToSteal
        };
    }

    public static ItemType GetRandomWeightedItem(ItemType[] validItems)
    {
        int totalWeight = 0;

        foreach (ItemType item in validItems)
            totalWeight += GetStealWeight(item);

        if (totalWeight <= 0)
            return ItemType.None;

        int randomValue = Random.Range(0, totalWeight);

        foreach (ItemType item in validItems)
        {
            randomValue -= GetStealWeight(item);

            if (randomValue < 0)
                return item;
        }

        return ItemType.None;
    }

    private static int GetStealWeight(ItemType item)
    {
        return item switch
        {
            ItemType.Coin => 30,
            ItemType.Flower => 25,
            ItemType.Branch => 25,
            ItemType.Log => 12,
            ItemType.Bouquet => 8,
            ItemType.Meat => 30,
            _ => 0
        };
    }


    public static bool IsTransferable(ItemType itemType)
    {
        return itemType != ItemType.Map && itemType != ItemType.Radar
            && itemType != ItemType.None;
    }

    public static bool IsStealable(ItemType itemType)
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
            ItemType.AncestralNecklace => true,
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