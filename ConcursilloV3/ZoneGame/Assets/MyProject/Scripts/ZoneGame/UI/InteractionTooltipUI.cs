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

    public void Show(string message)
    {
        interactionText.text = message;
        hasInteraction = true;

        UpdateVisibility(player.LocalState);
    }

    public void Clear()
    {
        interactionText.text = string.Empty;
        tooltipUI.SetActive(false);
        hasInteraction = false;
    }

    public void Hide()
    {
        tooltipUI.SetActive(false);
    }
}
