using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject errorPanel, joinGamePanel;
    private TMP_Text errorText;
    public static string PendingMessage { get; set; }

    private void Awake()
    {
        errorPanel.SetActive(false);
        joinGamePanel.SetActive(false);

        errorText = errorPanel.GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(PendingMessage))
        {
            errorPanel.SetActive(true);
            errorText.text = PendingMessage;
            PendingMessage = string.Empty;

            OpenErrorPanel();
        }
    }

    public void OpenErrorPanel()
    {
        errorPanel.SetActive(true);
    }

    public void CloseErrorPanel()
    {
        errorPanel.SetActive(false);
    }

    public void OpenJoinGamePanel()
    {
        joinGamePanel.SetActive(true);
    }

    public void CloseJoinGamePanel()
    {
        joinGamePanel.SetActive(false);
    }
}