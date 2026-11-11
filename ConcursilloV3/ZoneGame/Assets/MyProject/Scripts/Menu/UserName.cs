using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserName : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds4 = new WaitForSeconds(4f);
    private GameObject playerNameSetterPanel, playerNamePanel;
    private TMP_Text errorText;

    private TMP_InputField inputField;

    private Button quitAppBtn;

    private Coroutine errorCoroutine;

    private void Awake()
    {
        //PlayerPrefs.DeleteKey("PlayerName");

        playerNameSetterPanel = transform.Find("PlayerNameSetterPanel").gameObject;
        playerNamePanel = transform.Find("PlayerNamePanel").gameObject;
        errorText = playerNameSetterPanel.transform.Find("ErrorText").GetComponent<TMP_Text>();
        inputField = playerNameSetterPanel.GetComponentInChildren<TMP_InputField>();
        quitAppBtn = playerNameSetterPanel.transform.Find("QuitPanelBtn").GetComponent<Button>();

        errorText.gameObject.SetActive(false);
        playerNameSetterPanel.SetActive(!PlayerPrefs.HasKey("PlayerName"));

        SetPlayerName();
    }

    private void SetPlayerName()
    {
        TMP_Text playerNameText = playerNamePanel.transform.Find("PlayerName").GetComponent<TMP_Text>();

        playerNameText.text = PlayerPrefs.GetString("PlayerName", "Jugador");
    }

    public void SubmitName()
    {
        string playerName = inputField.text.Trim();

        if (string.IsNullOrWhiteSpace(playerName))
        {
            ShowErrorText("El nombre no es válido.");
            return;
        }

        PlayerPrefs.SetString("PlayerName", playerName);

        SetPlayerName();

        HidePanel();
    }

    public void ShowPanel()
    {
        playerNameSetterPanel.SetActive(true);
    }

    public void HidePanel()
    {
        if (!PlayerPrefs.HasKey("PlayerName"))
        {
            ShowErrorText("Debes introducir un nombre.");
            return;
        }

        playerNameSetterPanel.SetActive(false);
    }

    private void ShowErrorText(string text)
    {
        if (errorCoroutine != null)
            StopCoroutine(errorCoroutine);

        errorCoroutine = StartCoroutine(ShowErrorTextCoroutine(text));
    }

    private IEnumerator ShowErrorTextCoroutine(string text)
    {
        errorText.gameObject.SetActive(true);

        errorText.text = text;

        yield return _waitForSeconds4;

        errorText.gameObject.SetActive(false);

        errorCoroutine = null;
    }
}
