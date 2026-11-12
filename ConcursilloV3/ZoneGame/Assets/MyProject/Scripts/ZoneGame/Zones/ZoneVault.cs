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
            ZoneType.Tavern => new Tavern(),
            ZoneType.FlowerField => new FlowerField(),
            ZoneType.Ruins => new Ruins(),
            ZoneType.RabbitDen => new RabbitDen(),
            ZoneType.WolfDen => new WolfDen(),
            _ => null,
        };
    }

    public static string GetZoneType(ZoneType zoneType)
    {
        return zoneType switch
        {
            ZoneType.MomHouse => "Inicio",
            ZoneType.Forest => "Zona neutra",
            ZoneType.GrandmaHouse => "Objetivo",
            ZoneType.Swamp => "Zona peligrosa",
            ZoneType.Village => "Comercio",
            ZoneType.Camp => "Zona segura",
            ZoneType.VerticalRiver => "Obstáculo",
            ZoneType.HorizontalRiver => "Obstáculo",
            ZoneType.Tavern => "Zona neutra",
            ZoneType.FlowerField => "Recursos",
            ZoneType.Ruins => "Exploración",
            ZoneType.RabbitDen => "Ayuda",
            ZoneType.WolfDen => "Zona peligrosa",
            _ => "Desconocida"
        };
    }

    public static string GetZoneDescription(ZoneType zoneType)
    {
        return zoneType switch
        {
            ZoneType.MomHouse =>
                "Tu aventura comienza aquí, en la casa de tu madre.",

            ZoneType.Forest =>
                "Un bosque tranquilo sin efectos especiales.",

            ZoneType.GrandmaHouse =>
                "Entrega dos ramos de flores a la abuela para completar la aventura.",

            ZoneType.Swamp =>
                "Al entrar puedes perder objetos. Tendrás una oportunidad de recuperar objetos perdidos en el pantano.",

            ZoneType.Village =>
                "Compra y vende recursos utilizando monedas.",

            ZoneType.Camp =>
                "Un lugar seguro donde el lobo no puede entrar.",

            ZoneType.VerticalRiver or ZoneType.HorizontalRiver =>
                "Un río bloquea el camino. Necesitarás un tronco para construir un puente y cruzarlo.",

            ZoneType.Tavern =>
                "Una zona sin efectos especiales.",

            ZoneType.FlowerField =>
                "Un campo con mayor probabilidad de encontrar flores.",

            ZoneType.Ruins =>
                "Contiene un cofre con recompensas valiosas. Necesitas tres reliquias ancestrales para abrirlo.",

            ZoneType.RabbitDen =>
                "El conejo te permite teletransportarte a zonas que ya hayas descubierto.",

            ZoneType.WolfDen =>
                "La guarida del lobo, donde almacena los objetos robados a los jugadores.",

            _ => "No hay información disponible sobre esta zona."
        };
    }
}
