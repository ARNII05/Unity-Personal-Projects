using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestUI : MonoBehaviour
{
    public InventoryUI playerInventoryUI;
    public InventoryUI chestInventoryUI;
    private Vector3 playerInventoryBasePos = Vector3.zero;
    private Vector3 playerInventoryChangedPos = new(-460, -1);

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
        if (!player || !player.nearbyChest || !playerInventoryUI)
            return;

        ItemType itemType = playerInventoryUI.inventory.GetItemByIndex(index);

        if (!ItemVault.IsTransferable(itemType))
            return;

        if (!chestInventoryUI.inventory.HasSpaceForItem(itemType))
            return;

        player.SendItemsToChestServerRpc(itemType, 1);
    }

    public void SendAllItemsToChest(int index)
    {
        if (!player || !player.nearbyChest || !playerInventoryUI)
            return;

        ItemType itemType = playerInventoryUI.inventory.GetItemByIndex(index);
        int itemAmount = playerInventoryUI.inventory.GetItemAmount(itemType);

        if (!ItemVault.IsTransferable(itemType))
            return;

        if (!chestInventoryUI.inventory.HasSpaceForItem(itemType))
            return;

        player.SendItemsToChestServerRpc(itemType, itemAmount);
    }

    public void SendItemsToPlayer(int index)
    {
        if (!player || !player.nearbyChest || !playerInventoryUI)
            return;

        ItemType itemType = chestInventoryUI.inventory.GetItemByIndex(index);

        if (!ItemVault.IsTransferable(itemType))
            return;

        if (!playerInventoryUI.inventory.HasSpaceForItem(itemType))
            return;

        player.SendItemsToPlayerServerRpc(itemType, 1);
    }

    public void SendAllItemsToPlayer(int index)
    {
        if (!player || !player.nearbyChest || !playerInventoryUI)
            return;

        ItemType itemType = chestInventoryUI.inventory.GetItemByIndex(index);
        int itemAmount = chestInventoryUI.inventory.GetItemAmount(itemType);

        if (!ItemVault.IsTransferable(itemType))
            return;

        if (!playerInventoryUI.inventory.HasSpaceForItem(itemType))
            return;

        player.SendItemsToPlayerServerRpc(itemType, itemAmount);
    }

    public void ToggleInventory()
    {
        if (!chestInventoryUI.gameObject.transform.GetChild(0).gameObject.activeSelf)
            OpenInventory();
        else CloseInventory();
    }

    public void OpenInventory()
    {
        player.SetNewStateRpc(PlayerState.Trading);
        playerInventoryUI.transform.localPosition = playerInventoryChangedPos;
        
        playerInventoryUI.ChangeButtonStatus(true);
        chestInventoryUI.ChangeButtonStatus(true);
        
        playerInventoryUI.ShowInventory();
        chestInventoryUI.ShowInventory();
    }

    public void CloseInventory()
    {
        player.SetNewStateRpc(PlayerState.Normal);
        playerInventoryUI.transform.localPosition = playerInventoryBasePos;

        playerInventoryUI.ChangeButtonStatus(false);
        chestInventoryUI.ChangeButtonStatus(false);

        playerInventoryUI.HideInventory();
        chestInventoryUI.HideInventory();
    }
}
