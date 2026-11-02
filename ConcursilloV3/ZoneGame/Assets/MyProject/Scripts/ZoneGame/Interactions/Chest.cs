using Unity.Netcode;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private ChestType chestType = ChestType.Normal;
    public Inventory inventory = new();
    public ChestUI chestUI;

    public void Init(ChestUI chestUI)
    {
        this.chestUI = chestUI;

        if (chestType == ChestType.Ancient)
            transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Prefabs/Objects/AncientVaseClosed");
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

    public Sprite GetSprite()
    {
        return chestType == ChestType.Normal ? Resources.Load<Sprite>("Prefabs/Objects/OpenedBox") : Resources.Load<Sprite>("Prefabs/Objects/AncientVaseOpened");
    }
}

public enum ChestType
{
    Normal,
    Ancient
}