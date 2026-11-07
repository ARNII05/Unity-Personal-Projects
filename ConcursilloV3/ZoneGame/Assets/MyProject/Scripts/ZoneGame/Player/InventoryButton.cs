using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class InventoryButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int index;
    private ChestUI chestUI;
    public int toWho;

    public bool isBuffUI = false;

    private TooltipUI tooltipUI;

    private Player player;

    private bool isPointerOver;

    private void Awake()
    {
        GameObject obj = GameObject.Find("ChestUI");

        if (obj != null)
            chestUI = obj.GetComponent<ChestUI>();

        if (chestUI == null)
            Debug.LogError("No se ha encontrado ChestUI o le falta el componente.", this);

        tooltipUI = FindAnyObjectByType<TooltipUI>();

        StartCoroutine(SetPlayer());
    }

    IEnumerator SetPlayer()
    {
        while (MapManager.Instance == null ||
               (player = MapManager.Instance.GetLocalPlayer()) == null)
        {
            yield return null;
        }

        if (toWho == 1)
            StartCoroutine(WaitForChestInventory());
        else
            player.inventory.OnInventoryChanged += RefreshTooltip;
    }

    IEnumerator WaitForChestInventory()
    {
        while (chestUI == null || chestUI.chestInventoryUI.inventory == null)
            yield return null;
        
        chestUI.chestInventoryUI.inventory.OnInventoryChanged += RefreshTooltip;
    }

    private void RefreshTooltip()
    {
        if (isPointerOver && tooltipUI != null && tooltipUI.gameObject.activeSelf)
            ShowItemInTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isBuffUI)
            return;

        if (!chestUI)
        {
            Debug.Log("ChestUI es null");
            return;
        }

        switch (toWho)
        {
            case 0:
                if (eventData.button == PointerEventData.InputButton.Right)
                    chestUI.SendAllItemsToChest(index);
                else chestUI.SendItemsToChest(index);
                break;
            case 1:
                if (eventData.button == PointerEventData.InputButton.Right)
                    chestUI.SendAllItemsToPlayer(index);
                else chestUI.SendItemsToPlayer(index);
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        
        ShowItemInTooltip();
    }

    private void ShowItemInTooltip()
    {
        if (tooltipUI == null)
        {
            Debug.LogError("TooltipUI no encontrado en la escena.");
            return;
        }

        if ((toWho == 0 || toWho == 2) && player == null)
        {
            Debug.LogWarning("El jugador todavía no está inicializado.");
            return;
        }

        Inventory inventory = toWho == 0 || toWho == 2
            ? player.inventory
            : chestUI.chestInventoryUI.inventory;

        ItemType itemType = toWho == 0 || toWho == 1
            ? inventory.GetItemAt(index)
            : inventory.GetBuffAt(index);

        int itemAmount = ItemVault.IsBuff(itemType)
            ? 1
            : inventory.GetAmount(itemType);

        tooltipUI.ShowTooltip(itemType, itemAmount, GetComponent<RectTransform>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        
        if (tooltipUI != null)
            tooltipUI.HideTooltip();
    }

    private void OnDisable()
    {
        isPointerOver = false;

        if (tooltipUI != null)
            tooltipUI.HideTooltip();
    }

    private void OnDestroy()
    {
        if (toWho == 0 || toWho == 2)
        {
            if (player != null && player.inventory != null)
                player.inventory.OnInventoryChanged -= RefreshTooltip;
        }
        else if (toWho == 1)
        {
            if (chestUI != null && chestUI.chestInventoryUI != null && chestUI.chestInventoryUI.inventory != null)
                chestUI.chestInventoryUI.inventory.OnInventoryChanged -= RefreshTooltip;
        }
    }
}