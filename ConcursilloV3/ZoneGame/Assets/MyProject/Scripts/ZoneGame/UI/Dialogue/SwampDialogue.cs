using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwampDialogue : MonoBehaviour
{
    [SerializeField] private GameObject swampDialogue;

    private GameObject stolenItemBox, recoverItemBox, itemRecovered, stolenItems;

    private TextMeshProUGUI recoverSwampMsg, stolenSwampMsg;

    private Image stolenItemBoxItemImg, recoverItemBoxItemImg;

    private void Awake()
    {
        swampDialogue.SetActive(false);

        stolenItemBox = swampDialogue.transform.Find("StolenItemBox").gameObject;
        stolenSwampMsg = stolenItemBox.transform.Find("SwampMsg").GetComponent<TextMeshProUGUI>();
        stolenItems = stolenItemBox.transform.Find("StolenItems").gameObject;
        stolenItemBoxItemImg = stolenItems.transform.Find("ItemImage").GetComponent<Image>();

        recoverItemBox = swampDialogue.transform.Find("RecoverItemBox").gameObject;
        itemRecovered = recoverItemBox.transform.Find("ItemRecovered").gameObject;
        recoverSwampMsg = recoverItemBox.transform.Find("SwampMsg").GetComponent<TextMeshProUGUI>();
        recoverItemBoxItemImg = itemRecovered.transform.Find("ItemImage").GetComponent<Image>();
    }

    public void ShowStolenItems(ItemType[] items, int[] amount)
    {
        swampDialogue.SetActive(true);
        stolenItems.SetActive(items.Length > 0);
        stolenSwampMsg.gameObject.SetActive(items.Length <= 0);

        for (int i = 0; i < items.Length; i++)
        {
            TextMeshProUGUI amountText = stolenItemBoxItemImg.transform.GetComponentInChildren<TextMeshProUGUI>();
            
            stolenItemBoxItemImg.sprite = ItemVault.GetItemSprite(items[i]);
            amountText.text = $"x{amount[i]}";
        }
    }

    public void ShowRecoveredItem(ItemType item, int amount, RecoveryState recoveryState)
    {
        itemRecovered.SetActive(recoveryState == RecoveryState.ItemRecovered);
        recoverSwampMsg.gameObject.SetActive(recoveryState != RecoveryState.ItemRecovered);

        if (recoveryState != RecoveryState.ItemRecovered)
        {
            recoverSwampMsg.text = GetRecoveredItemMsg(recoveryState);
            return;
        }

        TextMeshProUGUI amountText = recoverItemBoxItemImg.transform.GetComponentInChildren<TextMeshProUGUI>();

        recoverItemBoxItemImg.sprite = ItemVault.GetItemSprite(item);
        amountText.text = $"x{amount}";
    }

    public void Close()
    {
        swampDialogue.SetActive(false);
    }

    private string GetRecoveredItemMsg(RecoveryState recoveryState)
    {
        return recoveryState switch
        {
            RecoveryState.NoItemsToRecover => "Rebuscas entre las aguas turbias, pero no encuentras nada que recuperar.",
            RecoveryState.ProbToRecoverFailed => "Buscas entre las aguas turbias, pero el pantano se queda con tus pertenencias.",
            RecoveryState.NoSpaceInInventory => "Encuentras tus pertenencias, pero no tienes espacio en el inventario para recuperarlas",
            _ => ""
        };
    }
}
