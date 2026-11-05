using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Text errorTexts;

    public static string PendingMessage { get; set; }

    private void Start()
    {
        errorTexts.text = PendingMessage;
        PendingMessage = string.Empty;
    }
}