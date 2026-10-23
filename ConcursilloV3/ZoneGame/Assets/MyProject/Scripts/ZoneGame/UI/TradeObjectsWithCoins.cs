using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeObjectsWithCoins : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds3 = new(3f);
    [SerializeField] private GameObject tradePanel;
    
    private int actualAmount = 1;
    private Coroutine adviceCoroutine;

    private readonly ItemType[] comercialItems = {ItemType.Flower, ItemType.Branch, ItemType.Meat, ItemType.None };

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

        adviceText.text = "";
    }

    public void Init(Player player)
    {
        this.player = player;

        tradePanel.SetActive(false);
        tradeObjectWithCoins.SetActive(false);
    }

    public void OpenTrade()
    {
        tradePanel.SetActive(true);
        tradeObjectWithCoins.SetActive(true);

        UpdateUI();
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

    public void DeselectItem()
    {
        actualAmount = 1;
        selectedItem = ItemType.None;
        gridUI.SetActive(false);
    }

    public void UpdateUI(bool callingInRpc = false)
    {
        if (!tradePanel.activeSelf)
            return;
        
        playerCoinsText.text = $"x{player.inventory.GetItemAmount(ItemType.Coin)}";
        UpdateComercialBox(callingInRpc);
        UpdateBuyBox();
    }

    private void UpdateComercialBoxItemGrid(int index)
    {
        foreach (Transform child in comercialBoxItemsGrid.transform)
        {
            GameObject background = child.Find("Background").gameObject;

            if (background == null)
                continue;

            background.SetActive(false);
        }

        ItemType newSelection = comercialItems[index];

        bool wasSameItem = selectedItem == newSelection;

        selectedItem = wasSameItem ? ItemType.None : newSelection;

        if (newSelection == ItemType.None)
            return;

        GameObject actualChild = comercialBoxItemsGrid.transform.GetChild(index).gameObject;
        GameObject actualBackground = actualChild.transform.Find("Background").gameObject;

        actualBackground.SetActive(!wasSameItem);
    }

    private void UpdateComercialBox(bool callingInRpc = false)
    {
        foreach (Transform child in comercialBoxItemsGrid.transform)
        {
            ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType), child.name);
            TextMeshProUGUI maxItemAmount = child.Find("MaxAmount").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemCost = child.Find("Coin").GetComponentInChildren<TextMeshProUGUI>();
            GameObject background = child.Find("Background").gameObject;

            if (background != null && !callingInRpc)
                background.SetActive(false);

            maxItemAmount.text = $"x{PurchaseSystem.Instance.GetAmountLeft(itemType)}";
            itemCost.text = $"x{PurchaseSystem.Instance.GetItemPrice(itemType)}";
        }
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
        coin.GetComponentInChildren<TextMeshProUGUI>().text = $"x{actualAmount * PurchaseSystem.Instance.GetItemPrice(selectedItem)}";

        UpdateBuyButton();
    }

    private void UpdateBuyButton()
    {
        CanvasGroup canvasGroup = buyBox.transform.Find("ButtonUI").GetComponentInChildren<CanvasGroup>();
        Button buyButton = canvasGroup.transform.Find("BuyButton").GetComponent<Button>();
        GameObject lockObj = buyBox.transform.Find("ButtonUI").Find("Lock").gameObject;

        PurchaseResult purchaseResult = PurchaseSystem.Instance.CanBuyItem(selectedItem, actualAmount, player);

        bool canBuyObject = purchaseResult == PurchaseResult.Success;

        lockObj.SetActive(!canBuyObject);

        canvasGroup.alpha = canBuyObject ? 1f : 0.4f;
        buyButton.interactable = canBuyObject;

        WriteOnAdviceText(purchaseResult);
    }

    public void WriteOnAdviceText(
        PurchaseResult purchaseResult,
        bool callingInRpc = false)
    {
        string newMessage = purchaseResult switch
        {
            PurchaseResult.InventoryFull => "Sin espacio en el inventario",
            PurchaseResult.NotEnoughCoins => "No tienes monedas suficientes",
            PurchaseResult.OutOfStock => "No hay stock suficiente",
            PurchaseResult.InvalidAmount => "Cantidad inválida",
            PurchaseResult.Success => callingInRpc ? "Se ha completado la compra" : "",
            _ => "Ha ocurrido un error"
        };

        if (adviceCoroutine != null)
        {
            StopCoroutine(adviceCoroutine);
            adviceCoroutine = null;
        }

        if (string.IsNullOrEmpty(newMessage))
        {
            adviceText.gameObject.SetActive(false);
            return;
        }

        adviceText.text = newMessage;
        adviceText.gameObject.SetActive(true);

        adviceCoroutine = StartCoroutine(HideAdviceText());
    }

    private IEnumerator HideAdviceText()
    {
        yield return _waitForSeconds3;
        
        adviceText.gameObject.SetActive(false);
        adviceCoroutine = null;
    }

    public void BuyItem()
    {
        PurchaseResult purchaseResult = PurchaseSystem.Instance.CanBuyItem(selectedItem, actualAmount, player);

        if (purchaseResult != PurchaseResult.Success)
            return;
        
        player.PurchaseItemRpc(selectedItem, actualAmount);
    }

    public void SumAmount()
    {
        if (selectedItem == ItemType.None)
            return;

        PurchaseResult purchaseResult = PurchaseSystem.Instance.CanBuyItem(selectedItem, actualAmount, player);

        if (purchaseResult != PurchaseResult.Success)
        {
            WriteOnAdviceText(purchaseResult);
            return;
        }

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
}