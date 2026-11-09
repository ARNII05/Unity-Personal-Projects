using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel, joinGamePanel;
    private TMP_Text infoText;
    public static string PendingMessage { get; set; }
    public static bool IsErrorMsg { get; set; }

    private const string errorColor = "#F31818";
    private const string infoColor = "#000000";

    private void Awake()
    {
        infoPanel.SetActive(false);
        joinGamePanel.SetActive(false);

        infoText = infoPanel.GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(PendingMessage))
        {
            infoPanel.SetActive(true);
            infoText.text = PendingMessage;
            infoText.color = IsErrorMsg ? HexToColor(errorColor) : HexToColor(infoColor);
            PendingMessage = string.Empty;

            OpenInfoPanel();
        }
    }

    public void OpenInfoPanel()
    {
        infoPanel.SetActive(true);
    }

    public void CloseInfoPanel()
    {
        infoPanel.SetActive(false);
    }

    public void OpenJoinGamePanel()
    {
        joinGamePanel.SetActive(true);
    }

    public void CloseJoinGamePanel()
    {
        joinGamePanel.SetActive(false);
    }

    private Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }
}