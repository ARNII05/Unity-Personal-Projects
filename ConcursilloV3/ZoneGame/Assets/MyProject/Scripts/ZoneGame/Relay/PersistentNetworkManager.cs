
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class PersistentNetworkManager : MonoBehaviour
{
    private bool isHandlingDisconnect;

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
        if (isHandlingDisconnect || NetworkManager.Singleton == null)
            return;

        Debug.Log(
            $"Desconexión detectada. clientId: {clientId}, " +
            $"LocalClientId: {NetworkManager.Singleton.LocalClientId}, " +
            $"IsServer: {NetworkManager.Singleton.IsServer}"
        );

        // CASO 1: Somos el cliente y se ha desconectado el host.
        if (!NetworkManager.Singleton.IsServer)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("El cliente ha perdido la conexión con el host.");

                ReturnToMainMenu("El host ha abandonado la partida");
            }

            return;
        }

        // CASO 2: Somos el host y el otro jugador abandona durante la partida.
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameStarted)
        {
            ReturnToMainMenu("El otro jugador ha abandonado la partida");
            return;
        }

        // CASO 3: Desconexión durante el lobby.
        // Conservamos el comportamiento que ya funcionaba.
        Player disconnectedPlayer = MapManager.Instance.GetPlayerById(clientId);

        if (disconnectedPlayer != null)
        {
            MapManager.Instance.UnregisterPlayer(disconnectedPlayer);
            StartCoroutine(WaitForResourcesGenerator());

            Debug.Log($"Jugador desconectado y desregistrado: {clientId}");
        }
    }

    private void ReturnToMainMenu(string message)
    {
        if (isHandlingDisconnect)
            return;

        isHandlingDisconnect = true;

        MainMenuUI.PendingMessage = message;

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene(0);
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
