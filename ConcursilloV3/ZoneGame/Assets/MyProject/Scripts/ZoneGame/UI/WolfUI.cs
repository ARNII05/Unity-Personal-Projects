using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WolfUI : MonoBehaviour
{
    [SerializeField] private GameObject wolfPanel;
    private GameObject canStealItems, cantStealItems, grid;

    private PlayerState previousPlayerState;

    private Player player;

    void Start()
    {
        wolfPanel.SetActive(false);
        canStealItems = wolfPanel.transform.Find("CanStealItems").gameObject;
        cantStealItems = wolfPanel.transform.Find("CantStealItems").gameObject;
        
        grid = canStealItems.transform.Find("Grid").gameObject;

        canStealItems.SetActive(false);
        cantStealItems.SetActive(false);
        grid.SetActive(true);
    }

    private void OpenUI()
    {
        player.State = PlayerState.CaughtByWolf;
        
        wolfPanel.SetActive(true);
    }

    public void CloseUI()
    {
        player.State = previousPlayerState;

        if (player.State == PlayerState.UsingRadar)
            RadarUI.Instance.PlayVideoAndSound();

        wolfPanel.SetActive(false);
    }

    public void ShowItemsStolen(List<ItemType> items, List<int> amount, Player player)
    {
        this.player = player;

        previousPlayerState = player.State;

        canStealItems.SetActive(false);
        cantStealItems.SetActive(false);
        
        if (player.inventory.GetItemAmount(ItemType.Radar) > 0)
            RadarUI.Instance.StopVideoAndSound();

        if (items.Count == 0)
            cantStealItems.SetActive(true);
        else CanStoleItemsUI(items, amount);

        OpenUI();
    }
    
    private void CanStoleItemsUI(List<ItemType> items, List<int> amount)
    {
        canStealItems.SetActive(true);
        
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            GameObject zell = grid.transform.GetChild(i).gameObject;
            zell.SetActive(false);
        }

        for (int i = 0; i < items.Count; i++)
        {
            GameObject zell = grid.transform.GetChild(i).gameObject;
            zell.SetActive(true);

            Image zellImg = zell.GetComponentInChildren<Image>();
            TextMeshProUGUI zellText = zell.GetComponentInChildren<TextMeshProUGUI>();

            zellImg.sprite = ItemVault.GenerateItem(items[i]).sprite;
            zellText.text = $"x {amount[i]}";
        }
    }
}