using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecoverySystemUI : MonoBehaviour
{
    private GameObject recoverySystemUI;

    private GameObject grid;

    private const float timeInPanel = 5f;

    private void Awake()
    {
        recoverySystemUI = transform.Find("RecoverySystemUI").gameObject;

        grid = recoverySystemUI.transform.Find("Grid").gameObject;

        recoverySystemUI.SetActive(false);
    }

    public IEnumerator ShowRecoverySystemPanelInfo(ItemType[] items, int[] amounts)
    {
        recoverySystemUI.SetActive(true);

        for (int i = 0; i < grid.transform.childCount; i++)
        {
            grid.transform.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (i >= grid.transform.childCount)
                break;

            GameObject zell = grid.transform.GetChild(i).gameObject;
            zell.SetActive(true);
            Image iconImg = zell.transform.GetComponentInChildren<Image>();
            TextMeshProUGUI zellText = zell.GetComponentInChildren<TextMeshProUGUI>();

            iconImg.sprite = ItemVault.GetItemSprite(items[i]);
            zellText.text = $"X{amounts[i]}";
        }

        yield return new WaitForSeconds(timeInPanel);

        recoverySystemUI.SetActive(false);
    }
}
