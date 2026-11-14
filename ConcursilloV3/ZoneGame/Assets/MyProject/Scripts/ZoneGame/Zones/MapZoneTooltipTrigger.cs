using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class MapZoneTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public Direction direction;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Cursor.lockState != CursorLockMode.None ||
            Mouse.current == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (!RectTransformUtility.RectangleContainsScreenPoint(
            rect, mousePos, null))
            return;

        MapUI.Instance.ShowZoneTooltip(direction, rect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MapUI.Instance.HideZoneTooltip();
    }

    private void OnDisable()
    {
        if (MapUI.Instance != null)
            MapUI.Instance.HideZoneTooltip();
    }
}