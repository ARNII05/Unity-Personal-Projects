using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class ZoneData
{
    public ZoneType type;
    public string name;
    public Sprite icon;
    public Inventory chestInventory = new();
    public bool chestOpened = false;
    public bool riverBridged = false;
    public bool canHaveChest = true;
    public int bouquetsInInventory = 0;
    public Direction firstRiverDirection = Direction.None;
    public Vector3[] player1EntryPoints = new Vector3[6];
    public Vector3[] player2EntryPoints = new Vector3[6];
    public Vector3 player1ZonePos;
    public Vector3 player2ZonePos;
    public Vector3 player1TpZonePos;
    public Vector3 player2TpZonePos;

    public ZoneData(ZoneType type, string name)
    {
        icon = Resources.Load<Sprite>($"Prefabs/ZoneIcons/{type}");
        this.type = type;
        this.name = name;

        player1EntryPoints[0] = new Vector3(-100.1f, 44.4f, -6.2f);
        player1EntryPoints[1] = new Vector3(-100.1f, -44.4f, -6.2f);
        player1EntryPoints[2] = new Vector3(47.3f, -0.2f, -6.2f);
        player1EntryPoints[3] = new Vector3(-144.2f, -0.2f, -6.2f);

        player2EntryPoints[0] = new Vector3(366.1f, 43.8f, -6.2f);
        player2EntryPoints[1] = new Vector3(366.1f, -46.2f, -6.2f);
        player2EntryPoints[2] = new Vector3(502.1f, -0.2f, -6.2f);
        player2EntryPoints[3] = new Vector3(332.1f, -0.2f, -6.2f);

        player1ZonePos = Vector3.zero;
        player2ZonePos = new Vector3(466.1f, 0, 0);

        player1TpZonePos = new Vector3(-54.7f, -3.14f, -6.2f);
        player2TpZonePos = new Vector3(416.1f, 0.8f, -6.2f);
    }

    public virtual Vector3 GetEntryPoint(Direction direction, Vector3[] entryPoints, Side side)
    {
        return direction switch
        {
            Direction.North => entryPoints[1],
            Direction.South => entryPoints[0],
            Direction.East => entryPoints[3],
            Direction.West => entryPoints[2],
            _ => Vector3.zero
        };
    }

    public Vector3[] GetPlayerEntryPoint(bool isPlayer1)
    {
        return isPlayer1 ? player1EntryPoints : player2EntryPoints;
    }

    public Vector3 GetPlayerTpPosition(bool isPlayer1)
    {
        return isPlayer1 ? player1TpZonePos : player2TpZonePos;
    }

    public abstract void OnEnter(Player player);

    public virtual ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            < 15 => ChestItems.None,    //15%
            < 70 => ChestItems.Coin,    //55%
            < 85 => ChestItems.Flower,  //15%
            < 100 => ChestItems.Branch, //15%
            _ => ChestItems.None
        };
    }
}

public class MomHouse : ZoneData
{
    public MomHouse() : base(ZoneType.MomHouse, "Casa de la Madre")
    {
        canHaveChest = false;
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a la casa de tu Madre. Se escuchan los árboles mover con el viento.");
    }
}

public class Forest : ZoneData
{
    public Forest() : base(ZoneType.Forest, "Bosque")
    {
        player1EntryPoints[0] = new Vector3(-49.2f, 39.4f, -6.2f);
        player1EntryPoints[1] = new Vector3(-49.2f, -41.4f, -6.2f);
        player1EntryPoints[2] = new Vector3(31.1f, 5.6f, -6.2f);
        player1EntryPoints[3] = new Vector3(-132.1f, 5.6f, -6.2f);

        player2EntryPoints[0] = new Vector3(417.1f, 51.8f, -6.2f);
        player2EntryPoints[1] = new Vector3(417.1f, -50.2f, -6.2f);
        player2EntryPoints[2] = new Vector3(503.1f, 3.7f, -6.2f);
        player2EntryPoints[3] = new Vector3(330.1f, 3.7f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f);
        player2ZonePos = new Vector3(417, 36.6f);

        player1TpZonePos = new Vector3(-64.9f, -4f, -6.2f);
        player2TpZonePos = new Vector3(421.1f, 9.8f, -6.2f);
    }

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando al Bosque. Se escuchan los árboles mover con el viento.");
    }
}

public class RabbitDen : ZoneData
{
    public RabbitDen() : base(ZoneType.RabbitDen, "Madriguera del conejo")
    {
        player1EntryPoints[0] = new Vector3(-49.2f, 39.4f, -6.2f);
        player1EntryPoints[1] = new Vector3(-49.2f, -41.4f, -6.2f);
        player1EntryPoints[2] = new Vector3(31.1f, 5.6f, -6.2f);
        player1EntryPoints[3] = new Vector3(-132.1f, 5.6f, -6.2f);

        player2EntryPoints[0] = new Vector3(417.1f, 51.8f, -6.2f);
        player2EntryPoints[1] = new Vector3(417.1f, -50.2f, -6.2f);
        player2EntryPoints[2] = new Vector3(503.1f, 3.7f, -6.2f);
        player2EntryPoints[3] = new Vector3(330.1f, 3.7f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f);
        player2ZonePos = new Vector3(417, 36.6f);

        player1TpZonePos = new Vector3(-64.9f, -4f, -6.2f);
        player2TpZonePos = new Vector3(404.1f, -6.2f, -6.2f);
    }

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a una Zona Especial.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            < 10 => ChestItems.None,   // 10 %
            < 70 => ChestItems.Coin,   // 60 %
            < 85 => ChestItems.Flower, // 15 %
            < 100 => ChestItems.Branch,// 15 %
            _ => ChestItems.None
        };
    }
}

public class Tavern : ZoneData
{
    public Tavern() : base(ZoneType.Tavern, "Taberna")
    {
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a la Taberna.");
    }
}

public class FlowerField : ZoneData
{
    public FlowerField() : base(ZoneType.FlowerField, "Campo florido")
    {
        player1EntryPoints[0] = new Vector3(-49.2f, 39.4f, -6.2f);
        player1EntryPoints[1] = new Vector3(-49.2f, -41.4f, -6.2f);
        player1EntryPoints[2] = new Vector3(31.1f, 5.6f, -6.2f);
        player1EntryPoints[3] = new Vector3(-132.1f, 5.6f, -6.2f);

        player2EntryPoints[0] = new Vector3(417.1f, 51.8f, -6.2f);
        player2EntryPoints[1] = new Vector3(417.1f, -50.2f, -6.2f);
        player2EntryPoints[2] = new Vector3(503.1f, 3.7f, -6.2f);
        player2EntryPoints[3] = new Vector3(330.1f, 3.7f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f);
        player2ZonePos = new Vector3(417, 36.6f);

        player1TpZonePos = new Vector3(-64.9f, -4f, -6.2f);
        player2TpZonePos = new Vector3(404.1f, -6.2f, -6.2f);
    }

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Campo Florido.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            < 5 => ChestItems.None,     // 5 %
            < 25 => ChestItems.Coin,    // 20 %
            < 80 => ChestItems.Flower,  // 55 %
            < 100 => ChestItems.Branch, // 20 %
            _ => ChestItems.None
        };
    }
}

public class Ruins : ZoneData
{
    public bool ancientRelicsRemoved = false;

    public Ruins() : base(ZoneType.Ruins, "Ruinas")
    {
        canHaveChest = false;

        player1EntryPoints[0] = new Vector3(-49.6f, 46.9f, -6.2f);
        player1EntryPoints[1] = new Vector3(-49.6f, -43.1f, -6.2f);
        player1EntryPoints[2] = new Vector3(36.4f, -1f, -6.2f);
        player1EntryPoints[3] = new Vector3(-136.6f, -1f, -6.2f);

        player2EntryPoints[0] = new Vector3(417.1f, 51.8f, -6.2f);
        player2EntryPoints[1] = new Vector3(417.1f, -50.2f, -6.2f);
        player2EntryPoints[2] = new Vector3(503.1f, 3.7f, -6.2f);
        player2EntryPoints[3] = new Vector3(330.1f, 3.7f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f);
        player2ZonePos = new Vector3(417, 36.6f);

        player1TpZonePos = new Vector3(-64.9f, -4f, -6.2f);
        player2TpZonePos = new Vector3(396f, -15.2f, -6.2f);
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a unas Ruinas.");
    }
}

public class Swamp : ZoneData
{
    public Dictionary<ItemType, int> internalInventory = new();

    public Swamp() : base(ZoneType.Swamp, "Pantano")
    {
        player1EntryPoints[2] = new Vector3(42.3f, -2.2f, -6.2f);
        player2EntryPoints[2] = new Vector3(503f, -2.3f, -6.2f);
        player1TpZonePos = new Vector3(-49.7f, -2.14f, -6.2f);
        player2TpZonePos = new Vector3(417f, -1.3f, -6.2f);
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Pantano.");
    }
}

public class Village : ZoneData
{
    public Village() : base(ZoneType.Village, "Pueblo")
    {
        player1EntryPoints[0] = new Vector3(-54.7f, 8.5f, -6.2f);
        player1EntryPoints[1] = new Vector3(-36.7f, -49.5f, -6.2f);
        player1EntryPoints[2] = new Vector3(43.3f, -11.5f, -6.2f);
        player1EntryPoints[3] = new Vector3(-141.7f, -29.5f, -6.2f);

        player2EntryPoints[0] = new Vector3(411f, 9.7f, -6.2f);
        player2EntryPoints[1] = new Vector3(429f, -50.3f, -6.2f);
        player2EntryPoints[2] = new Vector3(496f, -12.3f, -6.2f);
        player2EntryPoints[3] = new Vector3(339f, -29.3f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f);
        player2ZonePos = new Vector3(417, 36.6f);

        player1TpZonePos = new Vector3(-64.9f, -4f, -6.2f);
        player2TpZonePos = new Vector3(429.1f, -27.3f, -6.2f);
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Pueblo.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            < 10 => ChestItems.None,    //10%
            < 55 => ChestItems.Coin,    //45%
            < 75 => ChestItems.Flower,  //25%
            < 100 => ChestItems.Branch, //25%
            _ => ChestItems.None
        };
    }
}

public class Camp : ZoneData
{
    public Camp() : base(ZoneType.Camp, "Campamento")
    {
        player1EntryPoints[0] = new Vector3(-49.2f, 39.4f, -6.2f);
        player1EntryPoints[1] = new Vector3(-49.2f, -41.4f, -6.2f);
        player1EntryPoints[2] = new Vector3(31.1f, 5.6f, -6.2f);
        player1EntryPoints[3] = new Vector3(-132.1f, 5.6f, -6.2f);

        player2EntryPoints[0] = new Vector3(417.1f, 51.8f, -6.2f);
        player2EntryPoints[1] = new Vector3(417.1f, -50.2f, -6.2f);
        player2EntryPoints[2] = new Vector3(503.1f, 3.7f, -6.2f);
        player2EntryPoints[3] = new Vector3(330.1f, 3.7f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f);
        player2ZonePos = new Vector3(417, 36.6f);

        player1TpZonePos = new Vector3(-64.9f, -4f, -6.2f);
        player2TpZonePos = new Vector3(404.1f, -6.2f, -6.2f);
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Campamento.");
    }
}

public class VerticalRiver : ZoneData
{
    public VerticalRiver() : base(ZoneType.VerticalRiver, "Rio")
    {
        canHaveChest = false;
        icon = Resources.Load<Sprite>("Prefabs/ZoneIcons/River");

        //Left side entry
        player1EntryPoints[0] = new Vector3(-85.9f, 15.1f, -6.2f);
        player1EntryPoints[1] = new Vector3(-121.4f, -20.4f, -6.2f);
        //East and west
        player1EntryPoints[2] = new Vector3(44f, 31.6f, -6.2f);
        player1EntryPoints[3] = new Vector3(-136.6f, -11.9f, -6.2f);
        //Righ side entry
        player1EntryPoints[4] = new Vector3(26.6f, 46.1f, -6.2f);
        player1EntryPoints[5] = new Vector3(26.6f, -46.6f, -6.2f);

        //Left side entry
        player2EntryPoints[0] = new Vector3(381.1f, 11.8f, -6.2f);
        player2EntryPoints[1] = new Vector3(344.1f, -22.2f, -6.2f);
        //East and west
        player2EntryPoints[2] = new Vector3(499.1f, 22.8f, -6.2f);
        player2EntryPoints[3] = new Vector3(330.1f, -11.2f, -6.2f);

        //Righ side entry
        player2EntryPoints[4] = new Vector3(488.1f, 43.8f, -6.2f);
        player2EntryPoints[5] = new Vector3(488.1f, -38.2f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f);
        player2ZonePos = new Vector3(417f, 36.6f);
    }

    public override Vector3 GetEntryPoint(Direction direction, Vector3[] entryPoints, Side side)
    {
        return direction switch
        {
            Direction.North => side == Side.Left ? entryPoints[1] : entryPoints[5],
            Direction.South => side == Side.Left ? entryPoints[0] : entryPoints[4],
            Direction.East => entryPoints[3],
            Direction.West => entryPoints[2],
            _ => Vector3.zero
        };
    }

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Río.");
    }
}

public class HorizontalRiver : ZoneData
{
    public HorizontalRiver() : base(ZoneType.HorizontalRiver, "Rio")
    {
        canHaveChest = false;
        icon = Resources.Load<Sprite>("Prefabs/ZoneIcons/River");

        player1EntryPoints[0] = new Vector3(-49.2f, 39.4f, -6.2f);
        player1EntryPoints[1] = new Vector3(-49.2f, -41.4f, -6.2f);
        player1EntryPoints[2] = new Vector3(31.1f, 5.6f, -6.2f);
        player1EntryPoints[3] = new Vector3(-132.1f, 5.6f, -6.2f);

        player2EntryPoints[0] = new Vector3(380.1f, 13.8f, -6.2f);
        player2EntryPoints[1] = new Vector3(354.1f, -47.2f, -6.2f);
        player2EntryPoints[2] = new Vector3(503.1f, -12.2f, -6.2f);
        player2EntryPoints[3] = new Vector3(332.11f, -12.2f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f);
        player2ZonePos = new Vector3(417, 36.6f);

        player1TpZonePos = new Vector3(-64.9f, -4f, -6.2f);
        player2TpZonePos = new Vector3(422.1f, -11.2f, -6.2f);
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Río.");
    }
}

public class GrandmaHouse : ZoneData
{
    public GrandmaHouse() : base(ZoneType.GrandmaHouse, "Casa de la abuela")
    {
        canHaveChest = false;
    }

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a la Casa de la Abuela.");
    }
}

public class WolfDen : ZoneData
{
    public WolfDen() : base(ZoneType.WolfDen, "Guarida del lobo")
    {
        canHaveChest = false;
        
        player1EntryPoints[0] = new Vector3(-59.9f, 12.7f, -6.2f);
        player1EntryPoints[1] = new Vector3(-49.9f, -44.3f, -6.2f);
        player1EntryPoints[2] = new Vector3(34.1f, -7.2f, -6.2f);
        player1EntryPoints[3] = new Vector3(-133.9f, 5.6f, -6.2f);

        player2EntryPoints[0] = new Vector3(407.1f, 13.8f, -6.2f);
        player2EntryPoints[1] = new Vector3(417.1f, -44.2f, -6.2f);
        player2EntryPoints[2] = new Vector3(503.1f, -6.3f, -6.2f);
        player2EntryPoints[3] = new Vector3(328.1f, -6.3f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f);
        player2ZonePos = new Vector3(417, 36.6f);

        player1TpZonePos = new Vector3(-64.9f, -4f, -6.2f);
        player2TpZonePos = new Vector3(404.1f, -6.2f, -6.2f);
    }

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a la Guarida del Lobo.");
    }
}

public enum ZoneType
{
    MomHouse,
    Forest,
    RabbitDen,
    Tavern,
    FlowerField,
    Ruins,
    Swamp,
    Village,
    Camp,
    VerticalRiver,
    HorizontalRiver,
    GrandmaHouse,
    WolfDen
}