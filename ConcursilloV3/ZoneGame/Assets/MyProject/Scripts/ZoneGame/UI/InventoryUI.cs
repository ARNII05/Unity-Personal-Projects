using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private GameObject inventoryUI;
    
    public Inventory inventory;

    private Player player;

    private void Start()
    {
        inventoryUI = gameObject.transform.GetChild(0).gameObject;
        ChangeItemBoxStatus(false);
        inventoryUI.SetActive(false);
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

    private void ChangeItemBoxStatus(bool status)
    {
        for (int i = 0; i < Inventory.maxCapacity; i++)
        {
            GameObject actualBox = inventoryUI.transform.Find($"Box{i + 1}").gameObject;
            actualBox.GetComponent<Button>().interactable = status;

            UpdateBoxLock(actualBox);

            GameObject itemBox = actualBox.transform.Find("IconBox").gameObject;
            itemBox.SetActive(status);
        }
    }

    public void ChangeButtonStatus(bool status)
    {
        for (int i = 0; i < Inventory.maxCapacity; i++)
        {
            GameObject actualBox = inventoryUI.transform.Find($"Box{i + 1}").gameObject;
            actualBox.GetComponent<Button>().interactable = false;
        }

        if (!status)
            return;

        for (int i = 0; i < inventory.items.Count; i++)
        {
            GameObject actualBox = inventoryUI.transform.Find($"Box{i + 1}").gameObject;
            actualBox.GetComponent<Button>().interactable = status;
        }
    }

    private void Refresh()
    {
        for (int j = 0; j < Inventory.maxCapacity; j++)
        {
            GameObject box =
                inventoryUI.transform.Find($"Box{j + 1}").gameObject;

            GameObject iconBox =
                box.transform.Find("IconBox").gameObject;

            iconBox.SetActive(false);
        }

        int i = 0;

        foreach (var item in inventory.items)
        {
            UpdateItemBox(i, item.Key, item.Value);
            i++;
        }
    }

    private void UpdateItemBox(int index, ItemType itemType, int amount)
    {
        GameObject actualBox =
            inventoryUI.transform.Find($"Box{index + 1}").gameObject;

        GameObject iconBox =
            actualBox.transform.Find("IconBox").gameObject;

        TextMeshProUGUI itemBoxText =
            iconBox.GetComponentInChildren<TextMeshProUGUI>();

        UpdateBoxLock(actualBox, !IsTransferable(itemType));

        Image itemImage =
            iconBox.transform.Find("ItemIcon").GetComponent<Image>();

        itemImage.sprite = ItemVault.GenerateItem(itemType).sprite;
        itemBoxText.text = amount.ToString();

        iconBox.SetActive(true);
    }

    private void UpdateBoxLock(GameObject box, bool isTransferable = false)
    {
        Transform lockTrans = box.transform.Find("Lock");

        if (lockTrans == null)
            return;

        lockTrans.gameObject.SetActive(isTransferable);
    }

    private bool IsTransferable(ItemType itemType)
    {
        return itemType != ItemType.Map && itemType != ItemType.Radar
            && itemType != ItemType.None;
    }

    public void ToggleInventory()
    {
        if (!inventoryUI.activeSelf) OpenInventory();
        else CloseInventory();
    }

    public void OpenInventory()
    {
        player.State = PlayerState.Inventory;
        ShowInventory();
        //CursorManager.Instance.UnlockCursor();
    }

    public void CloseInventory()
    {
        player.State = PlayerState.Normal;
        HideInventory();
        //CursorManager.Instance.LockCursor();
    }

    public void ShowInventory()
    {
        inventoryUI.SetActive(true);
    }

    public void HideInventory()
    {
        inventoryUI.SetActive(false);
    }
}