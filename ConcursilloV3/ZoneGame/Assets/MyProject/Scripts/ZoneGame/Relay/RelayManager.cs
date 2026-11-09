
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button createRelayButton;
    [SerializeField] private Button joinRelayButton;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text errorJoiningText;

    [Header("Error Settings")]
    [SerializeField] private float errorDuration = 4f;

    private Coroutine errorCoroutine;

    private bool isLoadingRoleScene;
    private bool servicesReady;

    private float pingTimer;
    private UnityTransport transport;

    public static string CurrentJoinCode { get; private set; } = string.Empty;

    private void Update()
    {
        if (transport == null ||
            NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsListening)
            return;

        pingTimer += Time.deltaTime;

        if (pingTimer >= 1f)
        {
            pingTimer = 0f;

            if (NetworkManager.Singleton.IsServer)
            {
                foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if (clientId == NetworkManager.ServerClientId)
                        continue;

                    ulong ping = transport.GetCurrentRtt(clientId);

                    Debug.Log($"[HOST] RTT hacia cliente {clientId}: {ping} ms");
                }
            }
            else
            {
                ulong ping = transport.GetCurrentRtt(
                    NetworkManager.ServerClientId);

                Debug.Log($"[CLIENTE] RTT hacia el host: {ping} ms");
            }
        }
    }

    private async void Start()
    {
        errorJoiningText.text = "";
        errorJoiningText.gameObject.SetActive(false);

        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        createRelayButton.interactable = false;
        joinRelayButton.interactable = false;

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            servicesReady = true;

            createRelayButton.interactable = true;
            joinRelayButton.interactable = true;

            Debug.Log("Unity Services y Authentication preparados.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al inicializar Unity Services: {e}");

            ShowJoinError("No se han podido inicializar los servicios online.");
        }
    }

    public async void OnCreateRelayButtonClicked()
    {
        if (!servicesReady)
            return;

        createRelayButton.interactable = false;
        joinRelayButton.interactable = false;

        try
        {
            string code = await CreateRelay();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al crear Relay: {e}");

            ShowJoinError("No se ha podido crear la partida.");

            createRelayButton.interactable = true;
            joinRelayButton.interactable = true;
        }
    }

    public async void OnJoinRelayButtonClicked()
    {
        if (!servicesReady)
        {
            ShowJoinError("Los servicios online todavía no están preparados.");
            return;
        }

        string code = joinCodeInput.text.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            ShowJoinError("Introduce un código de partida.");
            return;
        }

        createRelayButton.interactable = false;
        joinRelayButton.interactable = false;

        try
        {
            await JoinRelay(code);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Error de Relay: {e}");

            ShowJoinError(
                "No se ha podido encontrar o acceder a la partida. Comprueba el código."
            );

            createRelayButton.interactable = true;
            joinRelayButton.interactable = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al unirse a Relay: {e}");

            ShowJoinError("No se ha podido conectar a la partida.");

            createRelayButton.interactable = true;
            joinRelayButton.interactable = true;
        }
    }

    public async Task<string> CreateRelay()
    {
        Allocation allocation =
            await RelayService.Instance.CreateAllocationAsync(1);

        Debug.Log($"Relay allocation creada: {allocation.Region}");

        string joinCode =
            await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(new RelayServerData(allocation, "dtls"));

        PersistentNetworkManager.Instance.PrepareForNewSession();

        if (!NetworkManager.Singleton.StartHost())
            throw new Exception("No se pudo iniciar el Host.");

        CurrentJoinCode = joinCode;

        Debug.Log($"Relay creado. Código: {joinCode}");

        LoadRoleScene();

        return joinCode;
    }

    private void LoadRoleScene()
    {
        if (isLoadingRoleScene)
            return;

        isLoadingRoleScene = true;

        Debug.Log("Host iniciado. Cargando ZoneMap...");

        var status = NetworkManager.Singleton.SceneManager.LoadScene(
            "ZoneMap",
            LoadSceneMode.Single
        );

        if (status != Unity.Netcode.SceneEventProgressStatus.Started)
        {
            Debug.LogError($"No se pudo iniciar la carga de ZoneMap. Estado: {status}");
            isLoadingRoleScene = false;
        }
    }

    public async Task JoinRelay(string joinCode)
    {
        JoinAllocation allocation =
            await RelayService.Instance.JoinAllocationAsync(joinCode);

        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(new RelayServerData(allocation, "dtls"));

        PersistentNetworkManager.Instance.PrepareForNewSession();

        if (!NetworkManager.Singleton.StartClient())
            throw new Exception("No se pudo iniciar el Client.");

        Debug.Log("Conexión de cliente iniciada.");
    }

    private void ShowJoinError(string message)
    {
        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine);
            errorCoroutine = null;
        }

        errorCoroutine = StartCoroutine(
            ShowJoinErrorCoroutine(message)
        );
    }

    private IEnumerator ShowJoinErrorCoroutine(string message)
    {
        errorJoiningText.text = message;
        errorJoiningText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(errorDuration);

        errorJoiningText.text = "";
        errorJoiningText.gameObject.SetActive(false);

        errorCoroutine = null;
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
        Debug.Log($"Cliente conectado correctamente. ID: {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.LogError($"Cliente desconectado. ID: {clientId}");
    }

    private void OnDestroy()
    {
        Debug.Log("[RelayManager] Se ha destruido RelayManager.");

        if (NetworkManager.Singleton == null)
        {
            Debug.Log("[RelayManager] NetworkManager.Singleton es NULL.");
            return;
        }

        Debug.Log(
            $"[RelayManager] NetworkManager activo: " +
            $"{NetworkManager.Singleton.name}, " +
            $"IsListening: {NetworkManager.Singleton.IsListening}"
        );
    }
}
