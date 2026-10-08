using UnityEngine;

[System.Serializable]
public abstract class ZoneData
{
    public ZoneType type;

    public Sprite icon;
    public Inventory chestInventory = new();
    public bool chestOpened = false;
    public bool riverBridged = false;
    public int bouquetsInInventory = 0;
    public Direction firstRiverDirection = Direction.None;
    public Vector3[] player1EntryPoints = new Vector3[6];
    public Vector3[] player2EntryPoints = new Vector3[6];
    public Vector3 player1ZonePos;
    public Vector3 player2ZonePos;

    public ZoneData(ZoneType type)
    {
        icon = Resources.Load<Sprite>($"Prefabs/ZoneIcons/{type}");
        this.type = type;

        player1EntryPoints[0] = new Vector3(-100.1f, 44.4f, -6.2f);
        player1EntryPoints[1] = new Vector3(-100.1f, -44.4f, -6.2f);
        player1EntryPoints[2] = new Vector3(47.3f, -0.2f, -6.2f);
        player1EntryPoints[3] = new Vector3(-144.2f, -0.2f, -6.2f);

        player2EntryPoints[0] = new Vector3(155f, 44.4f, -6.2f);
        player2EntryPoints[1] = new Vector3(155f, -44.4f, -6.2f);
        player2EntryPoints[2] = new Vector3(300f, 0.5f, -6.2f);
        player2EntryPoints[3] = new Vector3(108f, 0.5f, -6.2f);

        player1ZonePos = Vector3.zero;
        player2ZonePos = new Vector3(255, 0, 0);
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

    public abstract void OnEnter(Player player);

    public virtual ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            <= 15 => ChestItems.None,
            <= 50 => ChestItems.Coin,
            <= 70 => ChestItems.Flower,
            <= 100 => ChestItems.Branch,
            _ => throw new System.NotImplementedException(),
        };
    }

    public string GetName()
    {
        return type switch
        {
            ZoneType.GrandmaHouse => "Casa de la Abuela",
            ZoneType.MomHouse => "Casa de la Madre",
            ZoneType.Forest => "Bosque",
            ZoneType.SpecialZone => "Zona Especial",
            ZoneType.Tavern => "Taberna",
            ZoneType.FlowerField => "Campo Florido",
            ZoneType.Ruins => "Ruinas",
            ZoneType.Swamp => "Pantano",
            ZoneType.Village => "Pueblo",
            ZoneType.Camp => "Campamento",
            ZoneType.VerticalRiver => "Rio",
            ZoneType.HorizontalRiver => "Rio",
            _ => "Zona Desconocida",
        };
    }
}

public class MomHouse : ZoneData
{
    public MomHouse() : base(ZoneType.MomHouse)
    {
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a la casa de tu Madre. Se escuchan los árboles mover con el viento.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return ChestItems.None;
    }
}

public class Forest : ZoneData
{
    public Forest() : base(ZoneType.Forest)
    {
        player1EntryPoints[0] = new Vector3(-49.2f, 39.4f, -6.2f);
        player1EntryPoints[1] = new Vector3(-49.2f, -41.4f, -6.2f);
        player1EntryPoints[2] = new Vector3(31.1f, 5.6f, -6.2f);
        player1EntryPoints[3] = new Vector3(-132.1f, 5.6f, -6.2f);

        player2EntryPoints[0] = new Vector3(205f, 41.6f, -6.2f);
        player2EntryPoints[1] = new Vector3(205.2f, -39.8f, -6.2f);
        player2EntryPoints[2] = new Vector3(287.7f, 5.7f, -6.2f);
        player2EntryPoints[3] = new Vector3(121.5f, 5.7f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f, 0);
        player2ZonePos = new Vector3(206, 36.6f, 0);
    }

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando al Bosque. Se escuchan los árboles mover con el viento.");
    }
}

public class SpecialZone : ZoneData
{
    public SpecialZone() : base(ZoneType.SpecialZone)
    {
    }

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a una Zona Especial.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            <= 10 => ChestItems.None,  
            <= 70 => ChestItems.Coin,   
            <= 85 => ChestItems.Flower,
            <= 100 => ChestItems.Branch, 
            _ => throw new System.NotImplementedException(),
        };
    }
}

public class Tavern : ZoneData
{
    public Tavern() : base(ZoneType.Tavern)
    {
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a la Taberna.");
    }
}

public class FlowerField : ZoneData
{
    public FlowerField() : base(ZoneType.FlowerField)
    {
        player1EntryPoints[0] = new Vector3(-49.2f, 39.4f, -6.2f);
        player1EntryPoints[1] = new Vector3(-49.2f, -41.4f, -6.2f);
        player1EntryPoints[2] = new Vector3(31.1f, 5.6f, -6.2f);
        player1EntryPoints[3] = new Vector3(-132.1f, 5.6f, -6.2f);

        player2EntryPoints[0] = new Vector3(205f, 41.6f, -6.2f);
        player2EntryPoints[1] = new Vector3(205.2f, -39.8f, -6.2f);
        player2EntryPoints[2] = new Vector3(287.7f, 5.7f, -6.2f);
        player2EntryPoints[3] = new Vector3(121.5f, 5.7f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f, 0);
        player2ZonePos = new Vector3(206, 36.6f, 0);
    }

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Campo Florido.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            <= 5 => ChestItems.None,
            <= 15 => ChestItems.Coin,
            <= 75 => ChestItems.Flower,
            <= 100 => ChestItems.Branch,
            _ => throw new System.NotImplementedException(),
        };
    }
}

public class Ruins : ZoneData
{
    public Ruins() : base(ZoneType.Ruins)
    {
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a unas Ruinas.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            <= 50 => ChestItems.None,     
            <= 70 => ChestItems.Coin,   
            <= 85 => ChestItems.Flower, 
            <= 100 => ChestItems.Branch, 
            _ => throw new System.NotImplementedException(),
        };
    }
}

public class Swamp : ZoneData
{
    public Swamp() : base(ZoneType.Swamp)
    {
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Pantano.");
    }
}

public class Village : ZoneData
{
    public Village() : base(ZoneType.Village)
    {
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Pueblo.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            <= 10 => ChestItems.None,
            <= 55 => ChestItems.Coin,
            <= 75 => ChestItems.Flower,
            <= 100 => ChestItems.Branch,
            _ => throw new System.NotImplementedException(),
        };
    }
}

public class Camp : ZoneData
{
    public Camp() : base(ZoneType.Camp)
    {
        player1EntryPoints[0] = new Vector3(-49.2f, 39.4f, -6.2f);
        player1EntryPoints[1] = new Vector3(-49.2f, -41.4f, -6.2f);
        player1EntryPoints[2] = new Vector3(31.1f, 5.6f, -6.2f);
        player1EntryPoints[3] = new Vector3(-132.1f, 5.6f, -6.2f);

        player2EntryPoints[0] = new Vector3(205f, 41.6f, -6.2f);
        player2EntryPoints[1] = new Vector3(205.2f, -39.8f, -6.2f);
        player2EntryPoints[2] = new Vector3(287.7f, 5.7f, -6.2f);
        player2EntryPoints[3] = new Vector3(121.5f, 5.7f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f, 0);
        player2ZonePos = new Vector3(206, 36.6f, 0);
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Campamento.");
    }
}

public class VerticalRiver : ZoneData
{
    public VerticalRiver() : base(ZoneType.VerticalRiver)
    {
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
        player2EntryPoints[0] = new Vector3(169.7f, 11.9f, -6.2f);
        player2EntryPoints[1] = new Vector3(133.8f, -19.3f, -6.2f);
        //East and west
        player2EntryPoints[2] = new Vector3(299f, 26.2f, -6.2f);
        player2EntryPoints[3] = new Vector3(117.9f, -11.9f, -6.2f);

        //Righ side entry
        player2EntryPoints[4] = new Vector3(286f, 44.3f, -6.2f);
        player2EntryPoints[5] = new Vector3(282.6f, -42.4f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f, 0);
        player2ZonePos = new Vector3(206, 36.6f, 0);
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

    public override ChestItems ItemProbInChest(int prob)
    {
        return ChestItems.None;
    }
}

public class HorizontalRiver : ZoneData
{
    public HorizontalRiver() : base(ZoneType.HorizontalRiver)
    {
        icon = Resources.Load<Sprite>("Prefabs/ZoneIcons/River");

        player1EntryPoints[0] = new Vector3(-85.7f, 12.9f, -6.2f);
        player1EntryPoints[1] = new Vector3(-110.6f, -43.9f, -6.2f);
        player1EntryPoints[2] = new Vector3(43.7f, -10.5f, -6.2f);
        player1EntryPoints[3] = new Vector3(-141.3f, -10.5f, -6.2f);

        player2EntryPoints[0] = new Vector3(169.5f, 13.4f, -6.2f);
        player2EntryPoints[1] = new Vector3(144f, -48.9f, -6.2f);
        player2EntryPoints[2] = new Vector3(298.2f, -10.6f, -6.2f);
        player2EntryPoints[3] = new Vector3(111.9f, -10.6f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f, 0);
        player2ZonePos = new Vector3(206, 36.6f, 0);
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Río.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return ChestItems.None;
    }
}

public class GrandmaHouse : ZoneData
{
    public GrandmaHouse() : base(ZoneType.GrandmaHouse)
    {
    }

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a la Casa de la Abuela.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return ChestItems.None;
    }
}

public class WolfDen : ZoneData
{
    public WolfDen() : base(ZoneType.WolfDen)
    {
    }

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a la Guarida del Lobo.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return ChestItems.None;
    }
}

public enum ZoneType
{
    MomHouse,
    Forest,
    SpecialZone,
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