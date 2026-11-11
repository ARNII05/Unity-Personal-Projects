using Unity.Netcode;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    private GameObject pauseUI;

    private Player player;

    private PlayerState previousPlayerState;

    private ControlsUI controlsUI;

    private void Awake()
    {
        pauseUI = transform.GetChild(0).gameObject;
        pauseUI.SetActive(false);

        controlsUI = GameObject.Find("ControlsPanel").GetComponent<ControlsUI>();
    }

    public void InitPlayer(Player player)
    {
        this.player = player;
    }

    public void ShowPauseMenu()
    {
        if (player.State.Value is 
            PlayerState.CaughtByWolf or PlayerState.Transitioning
            or PlayerState.EndGame)
            return;

        previousPlayerState = player.State.Value;

        player.SetNewStateRpc(PlayerState.PauseMenu);
        pauseUI.SetActive(true);
    }

    public void ResumeGame()
    {
        if (controlsUI.PanelIsActive)
        {
            controlsUI.ClosePanel();
            return;
        }

        player.SetNewStateRpc(previousPlayerState);
        pauseUI.SetActive(false);
    }

    public void ApplicationQuit()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        Application.Quit();
    }

    public void ReturnToMainMenu()
    {
        PersistentNetworkManager.Instance.ReturnToMainMenu("", false);
    }
}
