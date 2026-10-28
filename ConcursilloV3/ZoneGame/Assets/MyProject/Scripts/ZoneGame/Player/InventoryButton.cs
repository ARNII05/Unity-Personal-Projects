using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryButton : MonoBehaviour, IPointerClickHandler
{
    public int index;
    private ChestUI chestUI;
    public int toWho;

    private void Awake()
    {
        GameObject obj = GameObject.Find("ChestUI");

        if (obj != null)
            chestUI = obj.GetComponent<ChestUI>();

        if (chestUI == null)
            Debug.LogError("No se ha encontrado ChestUI o le falta el componente.", this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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
}