
using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PersistentNetworkManager : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.gameObject != gameObject)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }


    private void OnClientDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        Player disconnectedPlayer = MapManager.Instance.GetPlayerById(clientId);

        if (disconnectedPlayer != null)
        {
            MapManager.Instance.UnregisterPlayer(disconnectedPlayer);
            StartCoroutine(WaitForResourcesGenerator());
            Debug.Log($"Jugador desconectado y desregistrado: {clientId}");
        }
    }

    private IEnumerator WaitForResourcesGenerator()
    {
        while (ResourcesGenerator.Instance == null)
        {
            yield return null;
        }

        ResourcesGenerator.Instance.canActivateRecoveryTimer = false;
    }
}
