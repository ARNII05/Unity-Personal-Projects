using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [SerializeField] private GameObject inventoryPanel;
    public bool IsOpen => inventoryPanel.activeSelf;
    private Inventory inventory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ChangeItemBoxStatus(false);
        inventoryPanel.SetActive(false);
    }

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;
        inventory.OnInventoryChanged += Refresh;
        Refresh();
    }

    private void ChangeItemBoxStatus(bool status)
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject actualBox = inventoryPanel.transform.Find($"Box{i + 1}").gameObject;
            GameObject itemBox = actualBox.transform.Find("IconBox").gameObject;
            itemBox.SetActive(status);
        }
    }

    private void Refresh()
    {
        int i = 0;

        foreach (var item in inventory.items)
        {
            if (item.Key != ItemType.Coin)
            {
                UpdateItemBox(i, item.Key, item.Value);
                i++;
                continue;
            }
            
            UpdateCoinAmount(item.Value);
        }
    }

    private void UpdateItemBox(int index, ItemType itemType, int amount)
    {
        GameObject actualBox =
            inventoryPanel.transform.Find($"Box{index + 1}").gameObject;

        GameObject iconBox =
            actualBox.transform.Find("IconBox").gameObject;

        TextMeshProUGUI itemBoxText =
            iconBox.GetComponentInChildren<TextMeshProUGUI>();

        Image itemImage =
            iconBox.transform.Find("ItemIcon").GetComponent<Image>();

        itemImage.sprite = ItemVault.GenerateItem(itemType).sprite;
        itemBoxText.text = amount.ToString();

        iconBox.SetActive(true);
    }

    private void UpdateCoinAmount(int amount)
    {
        GameObject coinsObject =
            inventoryPanel.transform.Find("CoinsUI").gameObject;

        TextMeshProUGUI coinsObjectText = 
            coinsObject.GetComponentInChildren<TextMeshProUGUI>();

        coinsObjectText.text = amount.ToString();
    }

    public void ToggleInventory()
    {
        if (!inventoryPanel.activeSelf) OpenInventory();
        else CloseInventory();
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        CursorManager.Instance.UnlockCursor();
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        CursorManager.Instance.LockCursor();
    }
}