using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryButton : MonoBehaviour, IPointerClickHandler
{
    public int index;
    public ChestUI chestUI;
    public int toWho;

    public void OnPointerClick(PointerEventData eventData)
    {
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