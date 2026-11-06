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

    private void OnDisable()
    {
        if (tooltipUI != null)
            tooltipUI.HideTooltip();
    }

    IEnumerator SetPlayer()
    {
        while (MapManager.Instance == null)
            yield return null;

        player = MapManager.Instance.GetLocalPlayer();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!chestUI)
        {
            Debug.Log("ChestUI es null");
            return;
        }

        if (isBuffUI)
            return;

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

        if (tooltipUI != null && tooltipUI.gameObject.activeSelf)
            ShowItemInTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowItemInTooltip();
    }

    private void ShowItemInTooltip()
    {
        if (tooltipUI == null)
        {
            Debug.LogError("TooltipUI no encontrado en la escena.");
            return;
        }

        if (toWho == 0 && player == null)
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
        if (tooltipUI != null)
            tooltipUI.HideTooltip();
    }   
}