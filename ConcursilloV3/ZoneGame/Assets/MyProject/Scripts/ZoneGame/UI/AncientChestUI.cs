using TMPro;
using UnityEngine;

public class AncientChestUI : MonoBehaviour
{
    public static AncientChestUI Instance { get; private set; }

    private GameObject ancientChestUI;

    private Player player;

    private PlayerState previousPlayerState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ancientChestUI = transform.GetChild(0).gameObject;
        ancientChestUI.SetActive(false);
    }

    public void InitPlayer(Player player)
    {
        this.player = player;
    }

    public void ShowPanel(int amountOfAncientRelics)
    {
        if (player.State.Value != PlayerState.CaughtByWolf &&
            player.State.Value != PlayerState.Transitioning)
        {
            previousPlayerState = player.State.Value;
        }

        player.SetNewStateRpc(PlayerState.AncientChestUI);

        ancientChestUI.SetActive(true);

        FillUI(amountOfAncientRelics);
    }

    private void FillUI(int amountOfAncientRelics)
    {
        int ancientRelicsPerChest = CraftingSystem.ancientRelicsToOpenAncientChest;

        TMP_Text mainText = ancientChestUI.transform.Find("MainText").GetComponent<TMP_Text>();
        TMP_Text itemAmountText = ancientChestUI.transform.Find("ItemBox").GetComponentInChildren<TMP_Text>();

        mainText.text = $"Se necesitan {ancientRelicsPerChest} reliquias para abrir el cofre ancestral";
        itemAmountText.text = $"{amountOfAncientRelics} / {ancientRelicsPerChest}";
    }

    public void HidePanel()
    {
        player.SetNewStateRpc(previousPlayerState);

        ancientChestUI.SetActive(false);
    }
}
