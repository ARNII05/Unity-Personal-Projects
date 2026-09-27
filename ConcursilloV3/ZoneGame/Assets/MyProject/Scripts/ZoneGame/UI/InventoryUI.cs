using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [SerializeField] private GameObject inventoryPanel;
    
    private Inventory inventory;
    private Player player;

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

    public void InitPlayer(Player player)
    {
        this.player = player;
        SetInventory(player.inventory);
    }

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;
        inventory.OnInventoryChanged += Refresh;
        Refresh();
    }

    private void ChangeItemBoxStatus(bool status)
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject actualBox = inventoryPanel.transform.Find($"Box{i + 1}").gameObject;
            GameObject itemBox = actualBox.transform.Find("IconBox").gameObject;
            itemBox.SetActive(status);
        }
    }

    private void Refresh()
    {
        for (int j = 0; j < 4; j++)
        {
            GameObject box =
                inventoryPanel.transform.Find($"Box{j + 1}").gameObject;

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

    public void ToggleInventory()
    {
        if (!inventoryPanel.activeSelf) OpenInventory();
        else CloseInventory();
    }

    public void OpenInventory()
    {
        player.State = PlayerState.Inventory;
        inventoryPanel.SetActive(true);
        //CursorManager.Instance.UnlockCursor();
    }

    public void CloseInventory()
    {
        player.State = PlayerState.Normal;
        inventoryPanel.SetActive(false);
        //CursorManager.Instance.LockCursor();
    }
}