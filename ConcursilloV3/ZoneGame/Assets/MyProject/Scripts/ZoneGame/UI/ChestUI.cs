using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestUI : MonoBehaviour
{
    [SerializeField] private InventoryUI playerInventoryUI;
    public InventoryUI chestInventoryUI;

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

        if (!chestInventoryUI.inventory.HasSpaceForItem(itemType))
            return;

        playerInventoryUI.ChangeButtonStatus(true);
        playerInventoryUI.inventory.RemoveItem(itemType, 1);
        player.SendItemToChestServerRpc(itemType, 1);
    }

    public void SendAllItemsToChest(int index)
    {
        ItemType itemType = playerInventoryUI.inventory.GetItemByIndex(index);

        if (itemType == ItemType.None)
            return;

        int itemAmount = playerInventoryUI.inventory.GetItemAmount(itemType);

        if (!chestInventoryUI.inventory.HasSpaceForItem(itemType))
            return;

        playerInventoryUI.ChangeButtonStatus(true);
        playerInventoryUI.inventory.RemoveItem(itemType, itemAmount);
        player.SendItemToChestServerRpc(itemType, itemAmount);
    }

    public void SendItemsToPlayer(int index)
    {
        ItemType itemType = chestInventoryUI.inventory.GetItemByIndex(index);

        if (itemType == ItemType.None)
            return;

        if (!playerInventoryUI.inventory.HasSpaceForItem(itemType))
            return;

        playerInventoryUI.ChangeButtonStatus(true);
        playerInventoryUI.inventory.AddItem(itemType, 1);
        player.RemoveItemToChestServerRpc(itemType, 1);
    }

    public void SendAllItemsToPlayer(int index)
    {
        ItemType itemType = chestInventoryUI.inventory.GetItemByIndex(index);

        if (itemType == ItemType.None)
            return;

        int itemAmount = chestInventoryUI.inventory.GetItemAmount(itemType);

        if (!playerInventoryUI.inventory.HasSpaceForItem(itemType))
            return;

        playerInventoryUI.ChangeButtonStatus(true);
        playerInventoryUI.inventory.AddItem(itemType, itemAmount);
        player.RemoveItemToChestServerRpc(itemType, itemAmount);
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
