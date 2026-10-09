using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrandmaUI : MonoBehaviour
{
    public static GrandmaUI Instance { get; private set; }

    [SerializeField] private GameObject grandmaPanel;

    private TextMeshProUGUI requirement1, requirement2;

    private Button acceptBtn;

    private GameObject denyTextObj;

    Player player;
    Player otherPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        grandmaPanel.SetActive(false);
        
        acceptBtn = grandmaPanel.transform.Find("Note").Find("AcceptBtn").GetComponent<Button>();

        denyTextObj = acceptBtn.transform.GetChild(0).gameObject;
        denyTextObj.SetActive(false);

        requirement1 = grandmaPanel.transform.Find("Note").Find("Requirement1").GetComponentInChildren<TextMeshProUGUI>();
        requirement2 = grandmaPanel.transform.Find("Note").Find("Requirement2").GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Init(Player player)
    {
        this.player = player;
        otherPlayer = MapManager.Instance.GetOtherPlayer(player);
    }

    public void OpenPanel()
    {
        Debug.Log("Panel activated");

        player.State = PlayerState.GrandmaDialogueInteraction;

        grandmaPanel.SetActive(true);
        
        int bouquetCount = player.inventory.GetItemAmount(ItemType.Bouquet);
        int bouquetsToWin = CraftingSystem.BouquetsToWin;
        bool canWin = CanWin(bouquetCount, bouquetsToWin);

        requirement1.text = $" ->    {bouquetCount} / {bouquetsToWin}";
        requirement2.text = player.NetworkMapPos.Value != otherPlayer.NetworkMapPos.Value
            ? $" ->    {player.name}"
            : $" ->    {player.name} / {otherPlayer.name}";

        denyTextObj.SetActive(!canWin);
        acceptBtn.interactable = canWin;
    }

    public void EndGame()
    {
        if (!CanWin(player.inventory.GetItemAmount(ItemType.Bouquet), CraftingSystem.BouquetsToWin))
            return;

        grandmaPanel.SetActive(false);
        player.ShowEndGanePanelServerRpc();
    }

    public void CloseUI()
    {
        player.State = PlayerState.Normal;
        grandmaPanel.SetActive(false);
    }

    private bool CanWin(int bouquetCount, int bouquetsToWin)
    {
        return bouquetCount >= bouquetsToWin
            && player.NetworkMapPos.Value == otherPlayer.NetworkMapPos.Value;
    }
}