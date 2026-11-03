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
        if (!player || !player.nearbyChest || !playerInventoryUI || !chestInventoryUI)
            return;

        if (chestInventoryUI.inventory == null ||
            playerInventoryUI.inventory == null)
            return;

        ItemType itemType = playerInventoryUI.inventory.GetItemAt(index);

        if (!ItemVault.IsTransferable(itemType))
            return;

        if (!chestInventoryUI.inventory.HasSpaceFor(itemType))
            return;

        player.SendItemsToChestServerRpc(itemType, 1);
    }

    public void SendAllItemsToChest(int index)
    {
        if (!player || !player.nearbyChest || !playerInventoryUI || !chestInventoryUI)
            return;

        if (chestInventoryUI.inventory == null ||
            playerInventoryUI.inventory == null)
            return;

        ItemType itemType = playerInventoryUI.inventory.GetItemAt(index);
        int itemAmount = playerInventoryUI.inventory.GetAmount(itemType);

        if (!ItemVault.IsTransferable(itemType))
            return;

        if (!chestInventoryUI.inventory.HasSpaceFor(itemType))
            return;

        player.SendItemsToChestServerRpc(itemType, itemAmount);
    }

    public void SendItemsToPlayer(int index)
    {
        if (!player || !player.nearbyChest || !playerInventoryUI || !chestInventoryUI)
            return;

        if (chestInventoryUI.inventory == null ||
            playerInventoryUI.inventory == null)
            return;

        ItemType itemType = chestInventoryUI.inventory.GetItemAt(index);

        if (!ItemVault.IsTransferable(itemType))
            return;

        if (!playerInventoryUI.inventory.HasSpaceFor(itemType))
            return;

        player.SendItemsToPlayerServerRpc(itemType, 1);
    }

    public void SendAllItemsToPlayer(int index)
    {
        if (!player || !player.nearbyChest || !playerInventoryUI || !chestInventoryUI)
            return;

        if (chestInventoryUI.inventory == null ||
            playerInventoryUI.inventory == null)
            return;

        ItemType itemType = chestInventoryUI.inventory.GetItemAt(index);
        int itemAmount = chestInventoryUI.inventory.GetAmount(itemType);

        if (!ItemVault.IsTransferable(itemType))
            return;

        if (!playerInventoryUI.inventory.HasSpaceFor(itemType))
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
