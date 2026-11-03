using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Chest : MonoBehaviour
{
    [SerializeField] private ChestType chestType;
    public Inventory inventory = new();
    public ChestUI chestUI;

    public void Init(ChestUI chestUI, bool isOpen)
    {
        this.chestUI = chestUI;

        GetComponent<SpriteRenderer>().sprite = GetSprite(isOpen);
    }

    public void OnOpen(Player player)
    {
        if (chestType == ChestType.Normal)
            OpenNormalChest(player);
        else if (chestType == ChestType.Ancient)
            OpenAncientChest(player);
    }

    private void OpenNormalChest(Player player)
    {
        chestUI.InitPlayer(player);
        chestUI.ToggleInventory();
        player.UpdateOpenedChestServerRpc();
    }

    private void OpenAncientChest(Player player)
    {
        player.TryToRemoveAncientRelicsRpc(chestType, player.NetworkMapPos.Value);
    }

    public Sprite GetSprite(bool isOpen = true)
    {
        return isOpen switch
        {
            true => chestType == ChestType.Normal ? Resources.Load<Sprite>("Prefabs/Objects/OpenedBox") : Resources.Load<Sprite>("Prefabs/Objects/AncientVaseOpened"),
            _ => chestType == ChestType.Normal ? Resources.Load<Sprite>("Prefabs/Objects/ClosedBox") : Resources.Load<Sprite>("Prefabs/Objects/AncientVaseClosed")
        };
    }
}

public enum ChestType
{
    Normal,
    Ancient
}