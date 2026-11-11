using UnityEngine;

public class ControlsUI : MonoBehaviour
{
    private GameObject controlsUI;

    public bool PanelIsActive => controlsUI.activeSelf;

    private void Awake()
    {
        controlsUI = transform.GetChild(0).gameObject;
        controlsUI.SetActive(false);
    }

    public void OpenPanel()
    {
        if (!PlayerPrefs.HasKey("PlayerName"))
            return;

        controlsUI.SetActive(true);
    }

    public void ClosePanel()
    {
        controlsUI.SetActive(false);
    }
}