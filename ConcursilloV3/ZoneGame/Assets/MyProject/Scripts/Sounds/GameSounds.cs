using System.Collections.Generic;
using UnityEngine;

public static class GameSounds
{
    private const string baseSoundPath = "Sounds/ZoneGame/";

    private static readonly Dictionary<SFXType, AudioClip> sounds = new();

    public static AudioClip GetSFXSound(SFXType soundType)
    {
        if (sounds.TryGetValue(soundType, out AudioClip audioClip))
            return audioClip;

        audioClip = Resources.Load<AudioClip>($"{baseSoundPath}{soundType}");

        if (audioClip == null)
        {
            Debug.LogWarning($"No se ha encontrado el sonido: {soundType}");
            return null;
        }

        sounds.Add(soundType, audioClip);

        return audioClip;
    }
}

public enum SFXType
{
    CraftingLog,
    CraftingBouquet,
    BridgeBuild,
    ChestOpen,
    Trading,
    Teleport,
    Footstep,
    ItemPickup,
    Notification,
    Victory,
    TradeFailed,
    WolfSteal,
    AncientChest,
    MapOpen
}
