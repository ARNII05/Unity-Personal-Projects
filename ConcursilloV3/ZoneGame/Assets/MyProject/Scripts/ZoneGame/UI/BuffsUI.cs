using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffsUI : MonoBehaviour
{
    private GameObject buffsUI, grid, noItems;

    private Player player;

    private Inventory inventory;

    private void Awake()
    {
        buffsUI = transform.Find("BuffsUI").gameObject;
        grid = buffsUI.transform.Find("Grid").gameObject;
        noItems = buffsUI.transform.Find("NoItems").gameObject;
        buffsUI.SetActive(false);
        grid.SetActive(false);
        noItems.SetActive(true);
    }

    public void SetPlayer(Player player)
    {
        this.player = player;
        SetInventory(player.inventory);
    }

    public void SetInventory(Inventory newInventory)
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;

        inventory = newInventory;

        if (inventory != null)
            inventory.OnInventoryChanged += Refresh;

        Refresh();
    }

    private void Refresh()
    {
        if (inventory.activeBuffs.Count == 0)
        {
            grid.SetActive(false);
            noItems.SetActive(true);
            return;
        }

        grid.SetActive(true);
        noItems.SetActive(false);

        foreach (Transform child in grid.transform)
            child.gameObject.SetActive(false);

        for (int i = 0; i < inventory.activeBuffs.Count; i++)
        {
            GameObject actualBox = grid.transform.GetChild(i).gameObject;
            actualBox.SetActive(true);

            ItemType buffType = inventory.GetBuffAt(i);

            if (buffType == ItemType.None)
            {
                Debug.LogWarning($"Invalid buff at index {i}.");
                continue;
            }

            actualBox.transform.Find("ItemIcon").GetComponent<Image>().sprite = ItemVault.GetItemSprite(buffType);
        }
    }

    public void ToggleBuffsUI()
    {
        if (!buffsUI.activeSelf)
            OpenBuffsUI();
        else CloseBuffsUI();
    }

    private void OpenBuffsUI()
    {
        buffsUI.SetActive(true);
        player.SetNewStateRpc(PlayerState.BuffsUI);
    }

    private void CloseBuffsUI()
    {
        buffsUI.SetActive(false);
        player.SetNewStateRpc(PlayerState.Normal);
    }
}
