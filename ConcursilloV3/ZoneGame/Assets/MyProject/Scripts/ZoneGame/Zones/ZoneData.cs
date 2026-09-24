using UnityEngine;

[System.Serializable]
public abstract class ZoneData
{
    public ZoneType type;

    public bool chestOpened;
    public int flowersRemaining;
    public bool riverBridged;
    public Vector3[] player1EntryPoints;
    public Vector3[] player2EntryPoints;
    public Vector3 player1ZonePos;
    public Vector3 player2ZonePos;

    public ZoneData(ZoneType type, int initialFlowers = 1)
    {
        player1EntryPoints = new Vector3[4];
        player2EntryPoints = new Vector3[4];
        this.type = type;
        chestOpened = false;
        flowersRemaining = initialFlowers;
        riverBridged = false;
    }

    public int FlowersAvailable()
    {
        return flowersRemaining;
    }

    public Vector3 GetEntryPoint(Direction direction, Vector3[] entryPoints)
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

    public virtual void OnOpenChest()
    {
        if (chestOpened)
        {
            LogManager.Log("El cofre de esta zona ya está vacío.");
            return;
        }

        chestOpened = true;
        LogManager.Log("Has abierto un cofre.");
    }

    public virtual void OnPickingUpFlowers()
    {
        if (flowersRemaining <= 0)
        {
            LogManager.Log("No quedan flores en esta zona.");
            return;
        }

        flowersRemaining--;
        LogManager.Log($"Recogiste una flor. Quedan {flowersRemaining} en esta zona.");
    }
}
public class MomHouse : ZoneData
{
    public MomHouse() : base(ZoneType.MomHouse, initialFlowers: 0)
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
}

public class Forest : ZoneData
{
    public Forest() : base(ZoneType.Forest, initialFlowers: 5)
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
    public SpecialZone() : base(ZoneType.SpecialZone, initialFlowers: 5)
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
}

public class WaterPit : ZoneData
{
    public WaterPit() : base(ZoneType.WaterPit, initialFlowers: 0)
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
    public FlowerField() : base(ZoneType.FlowerField, initialFlowers: 2)
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
}

public class Ruins : ZoneData
{
    public Ruins() : base(ZoneType.Ruins, initialFlowers: 0)
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
}

public class Swamp : ZoneData
{
    public Swamp() : base(ZoneType.Swamp, initialFlowers: 0)
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
    public Village() : base(ZoneType.Village, initialFlowers: 0)
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
}

public class Camp : ZoneData
{
    public Camp() : base(ZoneType.Camp, initialFlowers: 0)
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
    public VerticalRiver() : base(ZoneType.VerticalRiver, initialFlowers: 0)
    {
        player1EntryPoints[0] = new Vector3(-85.2f, 11.2f, -6.2f);
        player1EntryPoints[1] = new Vector3(-127.9f, -32.2f, -6.2f);
        player1EntryPoints[2] = new Vector3(-62.3f, 3.3f, -6.2f);
        player1EntryPoints[3] = new Vector3(-136.6f, -11.9f, -6.2f);

        player2EntryPoints[0] = new Vector3(169.7f, 11.9f, -6.2f);
        player2EntryPoints[1] = new Vector3(126.9f, -32.8f, -6.2f);
        player2EntryPoints[2] = new Vector3(192.3f, 3.3f, -6.2f);
        player2EntryPoints[3] = new Vector3(117.9f, -11.9f, -6.2f);

        player1ZonePos = new Vector3(-49.1f, 36.6f, 0);
        player2ZonePos = new Vector3(206, 36.6f, 0);
    }
    public override void OnEnter(Player player)
    {
        LogManager.Log("Entrando a un Río.");
    }
}

public class HorizontalRiver : ZoneData
{
    public HorizontalRiver() : base(ZoneType.HorizontalRiver, initialFlowers: 0)
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
}

public class GrandmaHouse : ZoneData
{
    public GrandmaHouse() : base(ZoneType.GrandmaHouse, initialFlowers: 0)
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