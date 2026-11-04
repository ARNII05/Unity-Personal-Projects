using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffsUI : MonoBehaviour
{
    private GameObject buffsUI, buffBox, grid, noItems;

    private Player player;

    private Inventory inventory;

    private void Awake()
    {
        buffsUI = transform.Find("BuffsUI").gameObject;
        grid = buffsUI.transform.Find("Grid").gameObject;
        noItems = buffsUI.transform.Find("NoItems").gameObject;
        buffBox = Resources.Load<GameObject>("Prefabs/Objects/BuffBox");
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
            Destroy(child.gameObject);

        for (int i = 0; i < Inventory.maxBuffs; i++)
        {
            GameObject actualBox = Instantiate(buffBox, grid.transform);
            GameObject itemBox = actualBox.transform.Find("ItemBox").gameObject;
            GameObject textPanel = actualBox.transform.Find("TextPanel").gameObject;
            TextMeshProUGUI itemName = textPanel.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI desc = textPanel.transform.Find("Desc").GetComponent<TextMeshProUGUI>();
            
            if (i < inventory.activeBuffs.Count)
            {
                ItemType buffType = inventory.GetBuffAt(i);
                itemBox.transform.Find("ItemIcon").GetComponent<Image>().sprite = ItemVault.GetItemSprite(buffType);
                itemName.text = ItemVault.GetItemName(buffType);
                desc.text = ItemVault.GetItemDescription(buffType);
            }
            else
            {
                itemBox.SetActive(false);
                itemName.text = "";
                desc.text = "";
            }
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
