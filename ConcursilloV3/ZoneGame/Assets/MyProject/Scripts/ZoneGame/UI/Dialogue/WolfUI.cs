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

    public void ShowItemsStolen(Dictionary<ItemType, int> stolenItems, Player player)
    {
        this.player = player;

        previousPlayerState = player.State;

        canStealItems.SetActive(false);
        cantStealItems.SetActive(false);
        
        if (player.inventory.GetItemAmount(ItemType.Radar) > 0)
            RadarUI.Instance.StopVideoAndSound();

        if (stolenItems.Count == 0)
            cantStealItems.SetActive(true);
        else CanStoleItemsUI(stolenItems);

        OpenUI();
    }
    
    private void CanStoleItemsUI(Dictionary<ItemType, int> stolenItems)
    {
        canStealItems.SetActive(true);
        
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            GameObject zell = grid.transform.GetChild(i).gameObject;
            zell.SetActive(false);
        }

        int j = 0;

        foreach (var item in stolenItems)
        {
            GameObject zell = grid.transform.GetChild(j).gameObject;
            zell.SetActive(true);

            Image zellImg = zell.GetComponentInChildren<Image>();
            TextMeshProUGUI zellText = zell.GetComponentInChildren<TextMeshProUGUI>();

            zellImg.sprite = ItemVault.GenerateItem(item.Key).sprite;
            zellText.text = $"x {item.Value}";

            j++;
        }
    }
}