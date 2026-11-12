using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZoneTooltipUI : MonoBehaviour
{
    private GameObject toolTipUI;

    private TextMeshProUGUI zoneName, zoneType, zoneDesc;

    private Image zoneImg;

    private RectTransform tooltipRect;
    private RectTransform parentRect;
    private Canvas canvas;

    [SerializeField] private float mouseOffset = 15f;

    private void Awake()
    {
        toolTipUI = transform.Find("TooltipUI").gameObject;
        tooltipRect = toolTipUI.GetComponent<RectTransform>();
        parentRect = tooltipRect.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>().rootCanvas;
        zoneName = toolTipUI.transform.Find("ZoneName").GetComponent<TextMeshProUGUI>();
        zoneType = toolTipUI.transform.Find("ZoneType").GetComponent<TextMeshProUGUI>();
        zoneDesc = toolTipUI.transform.Find("ZoneDesc").GetComponent<TextMeshProUGUI>();
        zoneImg = toolTipUI.transform.Find("ZoneImg").GetComponent<Image>();

        toolTipUI.SetActive(false);
    }


    private void UpdateTooltipPosition(RectTransform slotRect)
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        // Obtener las esquinas del slot.
        Vector3[] slotCorners = new Vector3[4];
        slotRect.GetWorldCorners(slotCorners);

        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(cam, slotCorners[0]);
        Vector2 topLeft = RectTransformUtility.WorldToScreenPoint(cam, slotCorners[1]);
        Vector2 topRight = RectTransformUtility.WorldToScreenPoint(cam, slotCorners[2]);
        Vector2 bottomRight = RectTransformUtility.WorldToScreenPoint(cam, slotCorners[3]);

        float centerX = (bottomLeft.x + bottomRight.x) / 2f;
        float centerY = (bottomLeft.y + topLeft.y + topRight.y + bottomRight.y) / 4f;

        // Obtener el tamaño real del tooltip.
        Vector3[] tooltipCorners = new Vector3[4];
        tooltipRect.GetWorldCorners(tooltipCorners);

        float tooltipWidth = Vector2.Distance(
            RectTransformUtility.WorldToScreenPoint(cam, tooltipCorners[0]),
            RectTransformUtility.WorldToScreenPoint(cam, tooltipCorners[3])
        );

        float tooltipHeight = Vector2.Distance(
            RectTransformUtility.WorldToScreenPoint(cam, tooltipCorners[0]),
            RectTransformUtility.WorldToScreenPoint(cam, tooltipCorners[1])
        );

        float offset = mouseOffset * canvas.scaleFactor;

        // Comprobar si cabe debajo del slot.
        bool showBelow = bottomLeft.y - tooltipHeight - offset >= 0;

        // Elegir posición vertical.
        float targetY = showBelow
            ? bottomLeft.y - tooltipHeight / 2f - offset
            : topLeft.y + tooltipHeight / 2f + offset;

        targetY += 30f;

        // Mantener el tooltip dentro de la pantalla.
        float halfWidth = tooltipWidth / 2f;
        float halfHeight = tooltipHeight / 2f;

        centerX = Mathf.Clamp(centerX, halfWidth, Screen.width - halfWidth);
        targetY = Mathf.Clamp(targetY, halfHeight, Screen.height - halfHeight);

        // Centrar el pivot.
        tooltipRect.pivot = new Vector2(0.5f, 0.5f);

        // Convertir coordenadas de pantalla a coordenadas locales.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            new Vector2(centerX, targetY),
            cam,
            out Vector2 localPoint
        );

        tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
        tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
        tooltipRect.anchoredPosition = localPoint;
    }


    public void ShowZoneTooltip(ZoneData zone, RectTransform slotRect)
    {
        if (zone == null)
        {
            HideTooltip();
            return;
        }

        toolTipUI.SetActive(true);

        UpdateTooltipPosition(slotRect);

        zoneName.text = zone.name;
        zoneType.text = ZoneVault.GetZoneType(zone.type);
        zoneDesc.text = ZoneVault.GetZoneDescription(zone.type);
        zoneImg.sprite = zone.icon;
    }

    public void HideTooltip()
    {
        toolTipUI.SetActive(false);
    }
}