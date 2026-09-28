using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestUI : MonoBehaviour
{
    [SerializeField] private InventoryUI playerInventoryUI;
    [SerializeField] private InventoryUI chestInventoryUI;

    private Vector3 playerInventoryBasePos = Vector3.zero;
    private Vector3 playerInventoryChangedPos = new(-460, -1, 0);

    private Player player;

    public void InitPlayer(Player player)
    {
        this.player = player;
    }

    public void SetInventory(Inventory inventory)
    {
        chestInventoryUI.SetInventory(inventory);
    }

    public void SendItemsToChest(int index)
    {
        ItemType itemType = playerInventoryUI.inventory.GetItemByIndex(index);

        if (itemType == ItemType.None)
            return;

        if (chestInventoryUI.inventory.HasSpaceForItem(itemType))
        {
            playerInventoryUI.inventory.RemoveItem(itemType, 1);
            chestInventoryUI.inventory.AddItem(itemType, 1);
        }

        playerInventoryUI.ChangeButtonStatus(true);
        chestInventoryUI.ChangeButtonStatus(true);
    }

    public void SendAllItemsToChest(int index)
    {
        ItemType itemType = playerInventoryUI.inventory.GetItemByIndex(index);

        if (itemType == ItemType.None)
            return;

        int itemAmount = playerInventoryUI.inventory.GetItemAmount(itemType);

        if (chestInventoryUI.inventory.HasSpaceForItem(itemType))
        {
            playerInventoryUI.inventory.RemoveItem(itemType, itemAmount);
            chestInventoryUI.inventory.AddItem(itemType, itemAmount);
        }

        playerInventoryUI.ChangeButtonStatus(true);
        chestInventoryUI.ChangeButtonStatus(true);
    }

    public void SendItemsToPlayer(int index)
    {
        Debug.Log("Entered SendItemsToPlayer func");
        
        ItemType itemType = chestInventoryUI.inventory.GetItemByIndex(index);

        if (itemType == ItemType.None)
        {
            Debug.Log("No item found");
            return;
        }

        if (playerInventoryUI.inventory.HasSpaceForItem(itemType))
        {
            chestInventoryUI.inventory.RemoveItem(itemType, 1);
            playerInventoryUI.inventory.AddItem(itemType, 1);
        }

        playerInventoryUI.ChangeButtonStatus(true);
        chestInventoryUI.ChangeButtonStatus(true);
    }

    public void SendAllItemsToPlayer(int index)
    {
        Debug.Log("Entered SendAllItemsToPlayer func");
        
        ItemType itemType = chestInventoryUI.inventory.GetItemByIndex(index);

        if (itemType == ItemType.None)
        {
            Debug.Log("No item found");
            return;
        }

        int itemAmount = chestInventoryUI.inventory.GetItemAmount(itemType);

        if (playerInventoryUI.inventory.HasSpaceForItem(itemType))
        {
            chestInventoryUI.inventory.RemoveItem(itemType, itemAmount);
            playerInventoryUI.inventory.AddItem(itemType, itemAmount);
        }

        playerInventoryUI.ChangeButtonStatus(true);
        chestInventoryUI.ChangeButtonStatus(true);
    }

    public void ToggleInventory()
    {
        if (!chestInventoryUI.gameObject.transform.GetChild(0).gameObject.activeSelf)
            OpenInventory();
        else CloseInventory();
    }

    public void OpenInventory()
    {
        player.State = PlayerState.Trading;
        playerInventoryUI.transform.localPosition = playerInventoryChangedPos;
        
        playerInventoryUI.ChangeButtonStatus(true);
        chestInventoryUI.ChangeButtonStatus(true);
        
        playerInventoryUI.ShowInventory();
        chestInventoryUI.ShowInventory();
    }

    public void CloseInventory()
    {
        player.State = PlayerState.Normal;
        playerInventoryUI.transform.localPosition = playerInventoryBasePos;

        playerInventoryUI.ChangeButtonStatus(false);
        chestInventoryUI.ChangeButtonStatus(false);

        playerInventoryUI.HideInventory();
        chestInventoryUI.HideInventory();
    }
}
