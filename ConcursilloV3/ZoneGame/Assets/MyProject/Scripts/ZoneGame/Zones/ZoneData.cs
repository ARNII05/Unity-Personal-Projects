using UnityEngine;

[System.Serializable]
public abstract class ZoneData
{
    public ZoneType type;

    public Inventory chestInventory = new();
    public bool chestOpened;
    public bool riverBridged;
    public Direction firstRiverDirection;
    public Vector3[] player1EntryPoints;
    public Vector3[] player2EntryPoints;
    public Vector3 player1ZonePos;
    public Vector3 player2ZonePos;

    public ZoneData(ZoneType type)
    {
        player1EntryPoints = new Vector3[6];
        player2EntryPoints = new Vector3[6];
        firstRiverDirection = Direction.None;
        chestOpened = false;
        this.type = type;
        riverBridged = false;
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
            <= 35 => ChestItems.Meat,
            <= 50 => ChestItems.Coin,
            <= 70 => ChestItems.Flower,
            <= 100 => ChestItems.Branch,
            _ => throw new System.NotImplementedException(),
        };
    }
}
public class MomHouse : ZoneData
{
    public MomHouse() : base(ZoneType.MomHouse)
    {
        player1EntryPoints[0] = new Vector3(-100.1f, 44.4f, -6.2f);
        player1EntryPoints[1] = new Vector3(-100.1f, -44.4f, -6.2f);
        player1EntryPoints[2] = new Vector3(47.3f, -0.2f, -6.2f);
        player1EntryPoints[3] = new Vector3(-144.2f, -0.2f, -6.2f);

        player2EntryPoints[0] = new Vector3(155f, 44.4f, -6.2f);
        player2EntryPoints[1] = new Vector3(155f, -44.4f, -6.2f);
        player2EntryPoints[2] = new Vector3(300f, 0.5f, -6.2f);
        player2EntryPoints[3] = new Vector3(108f, 0.5f, -6.2f);

        player1ZonePos = Vector3.zero;
        player2ZonePos = new Vector3(255, 0 , 0);
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

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando al Bosque. Se escuchan los árboles mover con el viento.");
    }
}

public class SpecialZone : ZoneData
{
    public SpecialZone() : base(ZoneType.SpecialZone)
    {
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

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a una Zona Especial.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            <= 10 => ChestItems.None,   
            <= 40 => ChestItems.Meat,  
            <= 70 => ChestItems.Coin,   
            <= 85 => ChestItems.Flower,
            <= 100 => ChestItems.Branch, 
            _ => throw new System.NotImplementedException(),
        };
    }
}

public class WaterPit : ZoneData
{
    public WaterPit() : base(ZoneType.WaterPit)
    {
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
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Pozo de Agua.");
    }
}

public class FlowerField : ZoneData
{
    public FlowerField() : base(ZoneType.FlowerField)
    {
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

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Campo Florido.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            <= 5 => ChestItems.None,
            <= 10 => ChestItems.Meat,
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
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a unas Ruinas.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            <= 50 => ChestItems.None,   
            <= 60 => ChestItems.Meat,   
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
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Pantano.");
    }
}

public class Village : ZoneData
{
    public Village() : base(ZoneType.Village)
    {
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
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Pueblo.");
    }

    public override ChestItems ItemProbInChest(int prob)
    {
        return prob switch
        {
            <= 10 => ChestItems.None,
            <= 20 => ChestItems.Meat,
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
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Campamento.");
    }
}

public class VerticalRiver : ZoneData
{
    public VerticalRiver() : base(ZoneType.VerticalRiver)
    {
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

    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a la Casa de la Abuela.");
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
    WaterPit,
    FlowerField,
    Ruins,
    Swamp,
    Village,
    Camp,
    VerticalRiver,
    HorizontalRiver,
    GrandmaHouse
}