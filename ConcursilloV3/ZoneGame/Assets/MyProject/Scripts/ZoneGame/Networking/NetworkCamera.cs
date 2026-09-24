using Unity.Netcode;
using UnityEngine;

public class NetworkCamera : MonoBehaviour
{
    [SerializeField] private Camera hostCamera;
    [SerializeField] private Camera clientCamera;

    private void Update()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsListening)
            return;

        bool isHost = NetworkManager.Singleton.IsHost;

        hostCamera.gameObject.SetActive(isHost);
        clientCamera.gameObject.SetActive(!isHost);

        enabled = false;
    }
}