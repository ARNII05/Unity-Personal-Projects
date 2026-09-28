using Unity.Netcode;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public Inventory inventory = new();
    private ChestUI chestUI;

    private void Start()
    {
        GameObject chestobj = GameObject.Find("ChestUI");
        chestUI = chestobj.GetComponent<ChestUI>();
    }

    public void OnOpen(Player player)
    {
        chestUI.InitPlayer(player);
        chestUI.SetInventory(inventory);
        chestUI.ToggleInventory();
    }
}

public struct ChestItems
{
    public ItemType[] itemTypes;
    public int[] amounts;

    public ChestItems(ItemType[] itemTypes, int[] amounts)
    {
        this.itemTypes = itemTypes;
        this.amounts = amounts;
    }
}