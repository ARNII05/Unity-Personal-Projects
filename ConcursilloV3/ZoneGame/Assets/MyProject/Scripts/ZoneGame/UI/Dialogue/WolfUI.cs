using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WolfUI : MonoBehaviour
{
    [SerializeField] private GameObject wolfPanel;
    private GameObject canStealItems, cantStealItems, noSpaceInChest, grid;

    private PlayerState previousPlayerState;

    private Player player;

    void Start()
    {
        wolfPanel.SetActive(false);
        canStealItems = wolfPanel.transform.Find("CanStealItems").gameObject;
        cantStealItems = wolfPanel.transform.Find("CantStealItems").gameObject;
        noSpaceInChest = wolfPanel.transform.Find("NoSpaceInChest").gameObject;

        grid = canStealItems.transform.Find("Grid").gameObject;

        grid.SetActive(true);
    }

    private void OpenUI()
    {
        player.SetNewStateRpc(PlayerState.CaughtByWolf);
        
        wolfPanel.SetActive(true);
    }

    public void CloseUI()
    {
        player.SetNewStateRpc(previousPlayerState);

        if (player.State.Value == PlayerState.UsingRadar)
            RadarUI.Instance.PlayVideoAndSound();

        wolfPanel.SetActive(false);
    }

    public void ShowItemsStolen(Dictionary<ItemType, int> stolenItems, Player player, bool chestIsFull)
    {
        this.player = player;

        previousPlayerState = player.State.Value == PlayerState.CaughtByWolf || player.State.Value == PlayerState.Transitioning 
            ? previousPlayerState : player.State.Value;

        canStealItems.SetActive(false);
        cantStealItems.SetActive(false);
        noSpaceInChest.SetActive(false);

        if (player.inventory.GetAmount(ItemType.Radar) > 0)
            RadarUI.Instance.StopVideoAndSound();

        switch (chestIsFull, stolenItems.Count == 0)
        {
            case (true, true):
                noSpaceInChest.SetActive(true);
                break;

            case (false, true):
                cantStealItems.SetActive(true);
                break;

            default:
                CanStoleItemsUI(stolenItems);
                break;
        }

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
            if (j >= grid.transform.childCount)
                break;

            GameObject zell = grid.transform.GetChild(j).gameObject;
            zell.SetActive(true);

            Image zellImg = zell.GetComponentInChildren<Image>();
            TextMeshProUGUI zellText = zell.GetComponentInChildren<TextMeshProUGUI>();

            zellImg.sprite = ItemVault.GetItemSprite(item.Key);
            zellText.text = $"x {item.Value}";

            j++;
        }
    }
}