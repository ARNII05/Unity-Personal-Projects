using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkUI : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        CursorManager.Instance.LockCursor();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        CursorManager.Instance.LockCursor();
    }

    public void EndGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}