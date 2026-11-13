using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChestUI : MonoBehaviour
{
    private InventoryUI playerInventoryUI;
    public InventoryUI chestInventoryUI;

    private Player player;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        playerInventoryUI = GameObject.Find("InventoryPanel").GetComponent<InventoryUI>();
        chestInventoryUI = transform.GetComponent<InventoryUI>();
    }

    public void InitPlayer(Player player)
    {
        this.player = player;
    }

    public void SetInventory(Inventory inventory)
    {
        chestInventoryUI.SetInventory(inventory);
        OnInventoryChanged?.Invoke();
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
        player.SetNewStateRpc(PlayerState.ChestInventory);
        
        playerInventoryUI.ChangeButtonStatus(true);
        chestInventoryUI.ChangeButtonStatus(true);
        
        playerInventoryUI.ShowInventory();
        chestInventoryUI.ShowInventory();
    }

    public void CloseInventory()
    {
        player.SetNewStateRpc(PlayerState.Normal);

        playerInventoryUI.ChangeButtonStatus(false);
        chestInventoryUI.ChangeButtonStatus(false);

        playerInventoryUI.HideInventory();
        chestInventoryUI.HideInventory();
    }
}
