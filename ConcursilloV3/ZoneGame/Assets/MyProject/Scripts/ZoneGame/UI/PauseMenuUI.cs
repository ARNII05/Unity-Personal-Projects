using Unity.Netcode;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    private GameObject pauseUI;

    private Player player;

    private PlayerState previousPlayerState;

    private void Awake()
    {
        pauseUI = transform.GetChild(0).gameObject;
        pauseUI.SetActive(false);
    }

    public void InitPlayer(Player player)
    {
        this.player = player;
    }

    public void ShowPauseMenu()
    {
        if (player.State.Value != PlayerState.CaughtByWolf &&
            player.State.Value != PlayerState.Transitioning)
        {
            previousPlayerState = player.State.Value;
        }

        player.SetNewStateRpc(PlayerState.PauseMenu);
        pauseUI.SetActive(true);
    }

    public void ResumeGame()
    {
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
