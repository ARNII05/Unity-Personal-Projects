using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WolfUI : MonoBehaviour
{
    [SerializeField] private GameObject wolfPanel;
    private GameObject canStealItems, cantStealItems, noSpaceInChest, grid, luckPotionEffect;

    private PlayerState previousPlayerState;

    private Player player;

    void Start()
    {
        wolfPanel.SetActive(false);
        canStealItems = wolfPanel.transform.Find("CanStealItems").gameObject;
        cantStealItems = wolfPanel.transform.Find("CantStealItems").gameObject;
        noSpaceInChest = wolfPanel.transform.Find("NoSpaceInChest").gameObject;
        luckPotionEffect = wolfPanel.transform.Find("LuckPotionEffect").gameObject;

        grid = canStealItems.transform.Find("Grid").gameObject;

        grid.SetActive(true);
    }

    private void OpenUI()
    {
        if (player.State.Value != PlayerState.CaughtByWolf &&
            player.State.Value != PlayerState.Transitioning)
        {
            previousPlayerState = player.State.Value;
        }

        player.SetNewStateRpc(PlayerState.CaughtByWolf);
        wolfPanel.SetActive(true);
    }

    public void CloseUI()
    {
        PlayerState stateToRestore = previousPlayerState;

        player.SetNewStateRpc(stateToRestore);

        if (stateToRestore == PlayerState.UsingRadar)
            RadarUI.Instance.PlayVideoAndSound();

        wolfPanel.SetActive(false);
    }

    public void ShowItemsStolen(ItemType[] stolenItems, int[] amounts, Player player, StealResult stealResult)
    {
        this.player = player;

        canStealItems.SetActive(false);
        cantStealItems.SetActive(false);
        noSpaceInChest.SetActive(false);
        luckPotionEffect.SetActive(false);

        if (player.inventory.GetAmount(ItemType.Radar) > 0)
            RadarUI.Instance.StopVideoAndSound();

        switch (stealResult, stolenItems.Length == 0 || amounts.Length == 0)
        {
            case (StealResult.ChestFull, true):
                noSpaceInChest.SetActive(true);
                break;

            case (StealResult.NoItemsToSteal, true):
                cantStealItems.SetActive(true);
                break;

            case (StealResult.LuckPotionEffect, true):
                luckPotionEffect.SetActive(true);
                break;

            default:
                CanStoleItemsUI(stolenItems, amounts);
                break;
        }

        OpenUI();
    }
    
    private void CanStoleItemsUI(ItemType[] stolenItems, int[] amounts)
    {
        canStealItems.SetActive(true);
        
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            GameObject zell = grid.transform.GetChild(i).gameObject;
            zell.SetActive(false);
        }

        for (int i = 0; i < stolenItems.Length; i++)
        {
            if (i >= grid.transform.childCount)
                break;

            GameObject zell = grid.transform.GetChild(i).gameObject;
            zell.SetActive(true);

            Image zellImg = zell.GetComponentInChildren<Image>();
            TextMeshProUGUI zellText = zell.GetComponentInChildren<TextMeshProUGUI>();

            zellImg.sprite = ItemVault.GetItemSprite(stolenItems[i]);
            zellText.text = $"x {amounts[i]}";
        }
    }
}