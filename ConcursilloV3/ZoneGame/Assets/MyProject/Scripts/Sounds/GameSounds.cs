
using UnityEngine;

public static class GameSounds
{
    private static AudioClip CraftingLog, CraftingBouquet, ChestOpen, Trading, Teleport;

    private static bool soundsInitialized = false;

    public static void InitSounds()
    {
        if (soundsInitialized)
            return;

        soundsInitialized = true;

        CraftingLog = Resources.Load<AudioClip>("Sounds/ZoneGame/CraftingLog");
        CraftingBouquet = Resources.Load<AudioClip>("Sounds/ZoneGame/CraftingBouquet");
        ChestOpen = Resources.Load<AudioClip>("Sounds/ZoneGame/ChestOpen");
        Trading = Resources.Load<AudioClip>("Sounds/ZoneGame/Trading");
        Teleport = Resources.Load<AudioClip>("Sounds/ZoneGame/Teleport");
    }

    public static AudioClip GetSFXSound(SFXType soundType)
    {
        return soundType switch
        {
            SFXType.CraftingBouquet => CraftingBouquet,
            SFXType.CraftingLog => CraftingLog,
            SFXType.ChestOpen => ChestOpen,
            SFXType.Trading => Trading,
            SFXType.Teleport => Teleport,
            _ => null
        };
    }
}

public enum SFXType
{
    CraftingLog,
    CraftingBouquet,
    BridgeBuild,
    ChestOpen,
    Trading,
    TradeFailed,
    Teleport,
    WolfSteal,
    AncientChest,
    Potion,
    MapOpen
}
