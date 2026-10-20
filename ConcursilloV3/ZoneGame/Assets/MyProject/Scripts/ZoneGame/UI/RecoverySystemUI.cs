using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecoverySystemUI : MonoBehaviour
{
    [SerializeField] private GameObject recoverySystemPanel;

    private GameObject grid;

    private const float timeInPanel = 5f;

    private void Awake()
    {
        grid = recoverySystemPanel.transform.Find("Grid").gameObject;
    }

    private void Start()
    {
        recoverySystemPanel.SetActive(false);
    }

    public IEnumerator ShowRecoverySystemPanelInfo(ItemType[] items, int[] amounts)
    {
        recoverySystemPanel.SetActive(true);

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

        recoverySystemPanel.SetActive(false);
    }
}
