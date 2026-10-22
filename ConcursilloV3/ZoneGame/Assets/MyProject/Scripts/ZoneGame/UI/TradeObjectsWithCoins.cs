using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeObjectsWithCoins : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    [SerializeField] private GameObject tradePanel;

    private const int flowerCost = 2;
    private const int branchCost = 4;
    private const int meatCost = 3;

    private const int maxFlowers = 5;
    private const int maxBranches = 5;
    private const int maxMeats = 5;

    private int flowersLeft = maxFlowers;
    private int branchesLeft = maxBranches;
    private int meatsLeft = maxMeats;

    private int actualAmount = 1;

    private ItemType[] comercialItems = {ItemType.Flower, ItemType.Branch, ItemType.Meat, ItemType.None };

    private ItemType selectedItem = ItemType.None;

    private GameObject tradeObjectWithCoins, comercialBox,
        comercialBoxItemsGrid, buyBox, gridUI, buyBoxItemsGrid;
    private TextMeshProUGUI playerCoinsText, adviceText;

    private Player player;

    private void Awake()
    {
        tradeObjectWithCoins = tradePanel.transform.Find("TradeObjectsWithCoins").gameObject;
        playerCoinsText = tradeObjectWithCoins.transform.Find("PlayerCoins").GetComponentInChildren<TextMeshProUGUI>();
        comercialBox = tradeObjectWithCoins.transform.Find("ComercialBox").gameObject;
        comercialBoxItemsGrid = comercialBox.transform.Find("ItemsGrid").gameObject;
        buyBox = comercialBox.transform.Find("BuyBox").gameObject;
        gridUI = buyBox.transform.Find("GridUI").gameObject;
        buyBoxItemsGrid = gridUI.transform.Find("ItemsGrid").gameObject;
        adviceText = buyBox.transform.Find("AdviceText").GetComponent<TextMeshProUGUI>();

        UpdateComercialBox();
        UpdateBuyBox();
    }

    public void Init(Player player)
    {
        this.player = player;

        tradePanel.SetActive(false);
        tradeObjectWithCoins.SetActive(false);

        player.inventory.OnInventoryChanged += UpdateCoinsText;
    }

    public void OpenTrade()
    {
        tradePanel.SetActive(true);
        tradeObjectWithCoins.SetActive(true);

        UpdateComercialBox();
        UpdateBuyBox();
    }
    
    public void ClosePanel()
    {
        tradePanel.SetActive(false);
        tradeObjectWithCoins.SetActive(false);
    }

    public void SelectItem(int index)
    {
        actualAmount = 1;
        
        UpdateComercialBoxItemGrid(index);
        UpdateBuyBox();
    }

    private void UpdateCoinsText()
    {
        playerCoinsText.text = $"x{player.inventory.GetItemAmount(ItemType.Coin)}";
    }

    private void UpdateComercialBoxItemGrid(int index)
    {
        foreach (Transform child in comercialBoxItemsGrid.transform)
        {
            GameObject background = child.Find("Background").gameObject;

            if (background == null)
                return;

            background.SetActive(false);
        }

        ItemType newSelection = comercialItems[index];

        GameObject actualChild = comercialBoxItemsGrid.transform.GetChild(index).gameObject;
        GameObject actualBackground = actualChild.transform.Find("Background").gameObject;

        bool wasSameItem = selectedItem == newSelection;

        selectedItem = wasSameItem ? ItemType.None : newSelection;
        actualBackground.SetActive(!wasSameItem);
    }

    private void UpdateBuyBox()
    {
        if (selectedItem == ItemType.None)
        {
            adviceText.gameObject.SetActive(false);
            gridUI.SetActive(false);
            return;
        }

        gridUI.SetActive(true);

        GameObject zell = buyBoxItemsGrid.transform.Find("Zell").gameObject;
        GameObject item = zell.transform.Find("Item").gameObject;
        GameObject coin = zell.transform.Find("Coin").gameObject;

        item.GetComponent<Image>().sprite = ItemVault.GetItemSprite(selectedItem);
        item.GetComponentInChildren<TextMeshProUGUI>().text = $"x{actualAmount}";
        coin.GetComponentInChildren<TextMeshProUGUI>().text = $"x{actualAmount * GetItemPrice(selectedItem)}";

        UpdateBuyButton();
    }

    private void UpdateBuyButton()
    {
        CanvasGroup canvasGroup = buyBox.transform.Find("ButtonUI").GetComponentInChildren<CanvasGroup>();
        Button buyButton = canvasGroup.transform.Find("BuyButton").GetComponent<Button>();
        GameObject lockObj = buyBox.transform.Find("ButtonUI").Find("Lock").gameObject;

        bool hasEnoughCoins = player.inventory.GetItemAmount(ItemType.Coin) >= GetItemPrice(selectedItem) * actualAmount;
        bool hasEnoughAmount = GetAmountLeft(selectedItem) >= actualAmount;
        bool hasSpaceInInventory = player.inventory.HasSpaceForItem(selectedItem);

        bool canBuyObject = hasEnoughCoins && hasEnoughAmount && hasSpaceInInventory;

        lockObj.SetActive(!canBuyObject);

        canvasGroup.alpha = canBuyObject ? 1f : 0.4f;
        buyButton.interactable = canBuyObject;

        if (!canBuyObject)
            StartCoroutine(WriteOnAdviceText(hasSpaceInInventory, hasEnoughCoins, hasEnoughAmount));
    }

    private IEnumerator WriteOnAdviceText(bool hasSpaceInInventory, bool hasEnoughCoins, bool hasEnoughAmount)
    {
        adviceText.gameObject.SetActive(true);

        adviceText.text = (hasSpaceInInventory, hasEnoughCoins, hasEnoughAmount) switch
        {
            (false, _, _) => "Sin espacio en el inventario",
            (true, false, _) => "No tienes monedas suficientes",
            (true, true, false) => "No hay stock suficiente",
            _ => ""
        };

        yield return _waitForSeconds1;

        adviceText.gameObject.SetActive(false);
    }

    private void UpdateComercialBox()
    {
        foreach (Transform child in comercialBoxItemsGrid.transform)
        {
            ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType), child.name);
            TextMeshProUGUI maxItemAmount = child.Find("MaxAmount").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemCost = child.Find("Coin").GetComponentInChildren<TextMeshProUGUI>();
            GameObject background = child.Find("Background").gameObject;

            if (background != null)
                background.SetActive(false);
            
            maxItemAmount.text = $"x{GetAmountLeft(itemType)}";
            itemCost.text = $"x{GetItemPrice(itemType)}";
        }
    }

    public void SumAmount()
    {
        if (selectedItem == ItemType.None)
            return;

        int amountLeft = GetAmountLeft(selectedItem);

        if (amountLeft == 0 || actualAmount >= amountLeft)
            return;

        actualAmount++;

        UpdateBuyBox();
    }

    public void ReduceAmount()
    {
        if (selectedItem == ItemType.None)
            return;

        if (actualAmount <= 1)
            return;

        actualAmount--;

        UpdateBuyBox();
    }

    private int GetItemPrice(ItemType item)
    {
        return item switch
        {
            ItemType.Flower => flowerCost,
            ItemType.Branch => branchCost,
            ItemType.Meat => meatCost,
            _ => 0
        };
    }

    private int GetAmountLeft(ItemType item)
    {
        return item switch
        {
            ItemType.Flower => flowersLeft,
            ItemType.Branch => branchesLeft,
            ItemType.Meat => meatsLeft,
            _ => 0
        };
    }
}