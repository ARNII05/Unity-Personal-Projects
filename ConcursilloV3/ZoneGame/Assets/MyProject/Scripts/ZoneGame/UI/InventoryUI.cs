using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private GameObject inventoryUI;
    
    public Inventory inventory;

    private Player player;

    private CraftingUI craftingUI;

    private GameObject itemTransferControlsGrid, boxesGrid;

    private int boxCount = 0;

    private void Awake()
    {
        inventoryUI = gameObject.transform.GetChild(0).gameObject;
        craftingUI = FindFirstObjectByType<CraftingUI>();
        itemTransferControlsGrid = inventoryUI.transform.Find("ItemTransferControlsGrid").gameObject;
        boxesGrid = inventoryUI.transform.Find("BoxesGrid").gameObject;

        itemTransferControlsGrid.SetActive(false);
        ChangeItemBoxStatus(false);
        inventoryUI.SetActive(false);

        boxCount = inventoryUI.transform
            .Cast<Transform>()
            .Count(child => child.name.StartsWith("Box"));
    }

    public void SetPlayer(Player player)
    {
        this.player = player;
        SetInventory(player.inventory);
        craftingUI.SetPlayer(player);
    }

    public void InitCraftingUI()
    {
        craftingUI.Init();
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

    private void ChangeItemBoxStatus(bool status)
    {
        for (int i = 0; i < Inventory.maxCapacity && i < boxCount; i++)
        {
            GameObject actualBox = boxesGrid.transform.Find($"Box{i + 1}").gameObject;
            actualBox.GetComponent<Button>().interactable = status;

            UpdateBoxLock(actualBox);

            GameObject itemBox = actualBox.transform.Find("IconBox").gameObject;
            itemBox.SetActive(status);
        }
    }

    public void ChangeButtonStatus(bool status)
    {
        for (int i = 0; i < Inventory.maxCapacity && i < boxCount; i++)
        {
            GameObject actualBox = boxesGrid.transform.Find($"Box{i + 1}").gameObject;
            actualBox.GetComponent<Button>().interactable = false;
        }

        if (!status)
            return;

        for (int i = 0; i < inventory.items.Count && i < boxCount; i++)
        {
            GameObject actualBox = boxesGrid.transform.Find($"Box{i + 1}").gameObject;
            actualBox.GetComponent<Button>().interactable = status;
        }
    }

    private void Refresh()
    {
        for (int j = 0; j < Inventory.maxCapacity && j < boxCount; j++)
        {
            GameObject box =
                boxesGrid.transform.Find($"Box{j + 1}").gameObject;

            GameObject iconBox =
                box.transform.Find("IconBox").gameObject;

            iconBox.SetActive(false);
        }

        for (int j = 0; j < Inventory.maxCapacity && j < boxCount; j++)
        {
            GameObject box =
                boxesGrid.transform.Find($"Box{j + 1}").gameObject;

            UpdateBoxLock(box, false);
        }

        int i = 0;

        foreach (var item in inventory.items)
        {
            if (i >= Inventory.maxCapacity)
                break;
            
            UpdateItemBox(i, item.Item, item.Amount);
            i++;
        }
    }

    private void UpdateItemBox(int index, ItemType itemType, int amount)
    {
        if (index >= boxCount)
            return;

        GameObject actualBox =
            boxesGrid.transform.Find($"Box{index + 1}").gameObject;

        GameObject iconBox =
            actualBox.transform.Find("IconBox").gameObject;

        TextMeshProUGUI itemBoxText =
            iconBox.GetComponentInChildren<TextMeshProUGUI>();

        UpdateBoxLock(actualBox, !ItemVault.IsTransferable(itemType));

        Image itemImage =
            iconBox.transform.Find("ItemIcon").GetComponent<Image>();

        itemImage.sprite = ItemVault.GetItemSprite(itemType);
        itemBoxText.text = amount.ToString();

        iconBox.SetActive(true);
    }

    private void UpdateBoxLock(GameObject box, bool isTransferable = false)
    {
        Transform lockTrans = box.transform.Find("IconBox").Find("Lock");

        if (lockTrans == null)
            return;

        lockTrans.gameObject.SetActive(isTransferable);
    }

    public void ToggleInventory()
    {
        if (!inventoryUI.activeSelf) ShowInventoryAndCrafting();
        else HideInventoryAndCrafting();
    }

    private void ShowInventoryAndCrafting()
    {
        player.SetState(PlayerState.Inventory);
        ShowInventory();
        craftingUI.OpenCraftingUI();
    }

    private void HideInventoryAndCrafting()
    {
        player.SetState(PlayerState.Normal);
        HideInventory();
        craftingUI.CloseCraftingUI();
    }

    public void ShowInventory(bool showGrid = false)
    {
        itemTransferControlsGrid.SetActive(showGrid);
        inventoryUI.SetActive(true);
    }

    public void HideInventory()
    {
        itemTransferControlsGrid.SetActive(false);
        inventoryUI.SetActive(false);
    }
}