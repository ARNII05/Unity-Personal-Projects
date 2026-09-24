using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ZoneVault
{
    public static ZoneData GenerateZone(ZoneType ZoneType)
    {
        return ZoneType switch
        {
            ZoneType.MomHouse => new MomHouse(),
            ZoneType.Forest => new Forest(),
            ZoneType.GrandmaHouse => new GrandmaHouse(),
            ZoneType.Swamp => new Swamp(),
            ZoneType.Village => new Village(),
            ZoneType.Camp => new Camp(),
            ZoneType.VerticalRiver => new VerticalRiver(),
            ZoneType.HorizontalRiver => new HorizontalRiver(),
            ZoneType.WaterPit => new WaterPit(),
            ZoneType.FlowerField => new FlowerField(),
            ZoneType.Ruins => new Ruins(),
            ZoneType.SpecialZone => new SpecialZone(),
            _ => null,
        };
    }
}
