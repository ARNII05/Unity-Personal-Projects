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

        Debug.Log($"===== CHEST CREATED ID: {GetInstanceID()} =====");

        FillInventory();
    }

    private void FillInventory()
    {
        int itemsToGenerate = Random.Range(1, 4);

        for (int i = 0; i < itemsToGenerate; i++)
        {
            ItemType randomItemType = (ItemType)Random.Range(1, System.Enum.GetValues(typeof(ItemType)).Length);

            Items generatedItem = ItemVault.GenerateItem(randomItemType);
            generatedItem.amount = Random.Range(1, 4);

            inventory.AddItem(randomItemType, generatedItem.amount);
        }
    }

    public void OnOpen(Player player)
    {
        Debug.Log($"===== CHEST OPENED ID: {GetInstanceID()} =====");

        foreach (var item in inventory.items)
        {
            Debug.Log($"OPENED CHEST -> {item.Key} x{item.Value}");
        }

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