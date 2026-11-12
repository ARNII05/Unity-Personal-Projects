using UnityEngine;

public class ScaleMapUI : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform mapImg;
    [SerializeField] private Vector2 referenceSize = new(1000, 800);

    private void OnEnable()
    {
        UpdateScale();
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateScale();
    }

    private void UpdateScale()
    {
        if (container == null || mapImg == null)
            return;

        if (referenceSize.x <= 0 || referenceSize.y <= 0)
            return;

        float scaleX = container.rect.width / referenceSize.x;
        float scaleY = container.rect.height / referenceSize.y;

        float scale = Mathf.Min(scaleX, scaleY);

        mapImg.localScale = Vector3.one * scale;
    }
}