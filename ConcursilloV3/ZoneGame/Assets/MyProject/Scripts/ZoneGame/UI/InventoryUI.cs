using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private int toWho;

    private GameObject inventoryUI;
    
    public Inventory inventory;

    private Player player;

    private CraftingUI craftingUI;

    private GameObject itemTransferControlsGrid, boxesGrid;

    private const string boxPrefabPath = "Prefabs/Objects/InventoryBox";

    private void Awake()
    {
        inventoryUI = gameObject.transform.GetChild(0).gameObject;
        craftingUI = FindFirstObjectByType<CraftingUI>();
        itemTransferControlsGrid = inventoryUI.transform.Find("ItemTransferControlsGrid").gameObject;
        boxesGrid = inventoryUI.transform.Find("BoxesGrid").gameObject;

        itemTransferControlsGrid.SetActive(false);
        ChangeItemBoxStatus(false);
        inventoryUI.SetActive(false);

        InitBoxesGrid();
    }

    private void InitBoxesGrid()
    {
        foreach (Transform child in boxesGrid.transform)
            Destroy(child.gameObject);

        GameObject boxPrefab = Resources.Load<GameObject>(boxPrefabPath);

        if (boxPrefab == null)
        {
            Debug.LogError($"Prefab not found in: {boxPrefabPath}");
            return;
        }

        for (int i = 0; i < Inventory.maxCapacity; i++)
        {
            GameObject box = Instantiate(boxPrefab, boxesGrid.transform);

            InventoryButton inventoryButton = box.GetComponent<InventoryButton>();

            inventoryButton.index = i;
            inventoryButton.toWho = toWho;
            inventoryButton.isBuffUI = false;
            
            box.name = $"Box{i + 1}";
        }
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
        for (int i = 0; i < Inventory.maxCapacity && i < boxesGrid.transform.childCount; i++)
        {
            GameObject actualBox = boxesGrid.transform.GetChild(i).gameObject;
            actualBox.GetComponent<Button>().interactable = status;

            UpdateBoxLock(actualBox);

            GameObject itemBox = actualBox.transform.Find("IconBox").gameObject;
            itemBox.SetActive(status);
        }
    }

    public void ChangeButtonStatus(bool status)
    {
        for (int i = 0; i < Inventory.maxCapacity && i < boxesGrid.transform.childCount; i++)
        {
            GameObject actualBox = boxesGrid.transform.GetChild(i).gameObject;
            actualBox.GetComponent<Button>().interactable = false;
        }

        if (!status)
            return;

        for (int i = 0; i < inventory.items.Count && i < boxesGrid.transform.childCount; i++)
        {
            GameObject actualBox = boxesGrid.transform.GetChild(i).gameObject;
            actualBox.GetComponent<Button>().interactable = status;
        }
    }

    private void Refresh()
    {
        for (int j = 0; j < Inventory.maxCapacity && j < boxesGrid.transform.childCount; j++)
        {
            GameObject box =
                boxesGrid.transform.GetChild(j).gameObject;

            GameObject iconBox =
                box.transform.Find("IconBox").gameObject;

            iconBox.SetActive(false);
        }

        for (int j = 0; j < Inventory.maxCapacity && j < boxesGrid.transform.childCount; j++)
        {
            GameObject box =
                boxesGrid.transform.GetChild(j).gameObject;

            UpdateBoxLock(box, false);
        }

        int i = 0;

        foreach (var item in inventory.items)
        {
            if (i >= boxesGrid.transform.childCount)
                break;

            UpdateItemBox(i, item.Item, item.Amount);
            i++;
        }
    }

    private void UpdateItemBox(int i, ItemType itemType, int amount)
    {
        if (i >= boxesGrid.transform.childCount)
            return;

        GameObject actualBox =
            boxesGrid.transform.GetChild(i).gameObject;

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