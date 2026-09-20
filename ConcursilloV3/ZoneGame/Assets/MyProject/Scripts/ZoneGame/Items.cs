using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items
{
    public ItemType itemType;
    public int amount;

    public Items(ItemType itemType, int amount)
    {
        this.itemType = itemType;
        this.amount = amount;
    }
}

public class Flower : Items
{
    public Flower() : base(ItemType.Flower, 0)
    {
    }
}

public class Log : Items
{
    public Log() : base(ItemType.Log, 0)
    {
    }
}


public class Meat : Items
{
    public Meat() : base(ItemType.Meat, 0)
    {
    }
}

public class Coin : Items
{
    public Coin() : base(ItemType.Coin, 0)
    {
    }
}

public class Radar : Items
{
    public Radar() : base(ItemType.Radar, 0)
    {
    }
}

public class Map : Items
{
    public Map() : base(ItemType.Map, 0)
    {
    }
}

public enum ItemType
{
    Flower,
    Log,
    Meat,
    Coin,
    Radar,
    Map
}
