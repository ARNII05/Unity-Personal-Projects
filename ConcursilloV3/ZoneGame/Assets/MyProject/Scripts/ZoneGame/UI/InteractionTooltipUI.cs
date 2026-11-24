using UnityEngine;
using TMPro;

public class InteractionTooltipUI : MonoBehaviour
{
    public static InteractionTooltipUI Instance { get; private set; }

    private GameObject tooltipUI;

    private TMP_Text interactionText;

    private bool hasInteraction;

    private Player player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        tooltipUI = transform.Find("TooltipUI").gameObject;
        interactionText = tooltipUI.transform.Find("InteractionText").GetComponent<TMP_Text>();
        interactionText.text = string.Empty;

        tooltipUI.SetActive(false);
    }

    public void Init(Player player)
    {
        this.player = player;
    }

    public void UpdateVisibility(PlayerState state)
    {
        if (tooltipUI == null)
            return;

        tooltipUI.SetActive(state == PlayerState.Normal && hasInteraction);
    }

    public void Clear()
    {
        hasInteraction = false;

        if (interactionText != null)
            interactionText.text = string.Empty;

        if (tooltipUI != null)
            tooltipUI.SetActive(false);
    }

    public void Hide()
    {
        if (tooltipUI != null)
            tooltipUI.SetActive(false);
    }

    public void Show(string message)
    {
        if (interactionText == null || tooltipUI == null || player == null)
            return;

        interactionText.text = message;
        hasInteraction = true;

        UpdateVisibility(player.LocalState);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
