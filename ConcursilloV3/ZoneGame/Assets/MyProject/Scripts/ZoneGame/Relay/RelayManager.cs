
using System;
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
    [SerializeField] private TMP_Text relayCodeText;
    [SerializeField] private TMP_InputField joinCodeInput;

    private bool servicesReady;

    private float pingTimer;
    private UnityTransport transport;


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
                // El host mide el RTT hacia cada cliente conectado.
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
                // El cliente mide el RTT hacia el host.
                ulong ping = transport.GetCurrentRtt(
                    NetworkManager.ServerClientId);

                Debug.Log($"[CLIENTE] RTT hacia el host: {ping} ms");
            }
        }
    }


    private async void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        createRelayButton.interactable = false;
        joinRelayButton.interactable = false;
        relayCodeText.text = "Inicializando servicios...";

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            servicesReady = true;
            createRelayButton.interactable = true;
            joinRelayButton.interactable = true;
            relayCodeText.text = "Servicios listos";
            Debug.Log("Unity Services y Authentication preparados.");
        }
        catch (Exception e)
        {
            relayCodeText.text = "Error al inicializar servicios";
            Debug.LogError($"Error al inicializar Unity Services: {e}");
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
            relayCodeText.text = $"Código: {code}";
        }
        catch (Exception e)
        {
            relayCodeText.text = "Error al crear partida";
            Debug.LogError($"Error al crear Relay: {e}");
            createRelayButton.interactable = true;
            joinRelayButton.interactable = true;
        }
    }

    public async void OnJoinRelayButtonClicked()
    {
        if (!servicesReady)
            return;

        string code = joinCodeInput.text.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            relayCodeText.text = "Introduce un código";
            return;
        }

        createRelayButton.interactable = false;
        joinRelayButton.interactable = false;

        try
        {
            await JoinRelay(code);
            relayCodeText.text = "Conectando...";
        }
        catch (Exception e)
        {
            relayCodeText.text = "Error al unirse";
            Debug.LogError($"Error al unirse a Relay: {e}");
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

        if (!NetworkManager.Singleton.StartHost())
            throw new Exception("No se pudo iniciar el Host.");

        Debug.Log($"Relay creado. Código: {joinCode}");
        return joinCode;
    }

    public async Task JoinRelay(string joinCode)
    {
        JoinAllocation allocation =
            await RelayService.Instance.JoinAllocationAsync(joinCode);

        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(new RelayServerData(allocation, "dtls"));

        if (!NetworkManager.Singleton.StartClient())
            throw new Exception("No se pudo iniciar el Client.");

        Debug.Log("Conexión de cliente iniciada.");
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

        if (NetworkManager.Singleton.IsClient &&
            NetworkManager.Singleton.LocalClientId == clientId)
        {
            relayCodeText.text = "¡Conectado correctamente!";
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.LogError($"Cliente desconectado. ID: {clientId}");
    }
}
