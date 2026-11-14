
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class PersistentNetworkManager : MonoBehaviour
{
    public static PersistentNetworkManager Instance { get; private set; }

    private bool isHandlingDisconnect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

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
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null)
            return;

        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;

        isHandlingDisconnect = false;

        Debug.Log("Nueva sesión iniciada. Gestión de desconexiones reiniciada.");
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

        if (MapManager.Instance == null)
        {
            Debug.LogWarning(
                "Se ha desconectado un jugador antes de inicializar MapManager."
            );
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

    public void ReturnToMainMenu(string message = "", bool isErrorMsg = true)
    {
        if (isHandlingDisconnect)
            return;

        isHandlingDisconnect = true;

        MainMenuUI.PendingMessage = message;
        MainMenuUI.IsErrorMsg = isErrorMsg;

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene(0);
    }

    public void PrepareForNewSession()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("No se puede preparar una sesión mientras hay otra activa.");
            return;
        }

        isHandlingDisconnect = false;

        Debug.Log("Gestión de desconexiones preparada para una nueva sesión.");
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
