using UnityEngine;
using UnityEngine.EventSystems;

public class RightClickHandler : MonoBehaviour, IPointerClickHandler
{
    public System.Action onRightClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            onRightClick?.Invoke();
        }
    }
}