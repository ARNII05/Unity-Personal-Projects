using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class MapZoneTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public Direction direction;

    public void OnPointerEnter(PointerEventData eventData)
    {
        MapUI.Instance.ShowZoneTooltip(
            direction,
            GetComponent<RectTransform>()
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MapUI.Instance.HideZoneTooltip();
    }
}