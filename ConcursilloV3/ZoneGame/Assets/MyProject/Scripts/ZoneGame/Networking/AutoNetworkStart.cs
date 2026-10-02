using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class AutoNetworkStart : MonoBehaviour
{
    private const string HostAddress = "127.0.0.1";
    private const ushort Port = 7777;

    private UnityTransport transport;
    private bool clientConnected;

    private void Start()
    {
        transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetConnectionData(
            HostAddress,
            Port
        );

        NetworkManager.Singleton.OnClientConnectedCallback +=
            OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback +=
            OnClientDisconnected;

#if UNITY_EDITOR
        StartHost();
#else
        StartCoroutine(TryConnectToHost());
#endif
    }

    private void StartHost()
    {
        Debug.Log("Iniciando Host...");

        bool started =
            NetworkManager.Singleton.StartHost();

        if (!started)
        {
            Debug.LogError("No se pudo iniciar el Host.");
        }
    }

    private IEnumerator TryConnectToHost()
    {
        while (!clientConnected)
        {
            Debug.Log("Intentando conectar al Host...");

            NetworkManager.Singleton.StartClient();

            float timeout = 2f;

            while (
                !clientConnected &&
                timeout > 0f
            )
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (clientConnected)
                yield break;

            Debug.Log("Host no disponible. Reintentando...");

            NetworkManager.Singleton.Shutdown();

            yield return new WaitForSeconds(1f);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId !=
            NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        clientConnected = true;

        Debug.Log("¡Conectado al Host!");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId !=
            NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        clientConnected = false;

        Debug.Log("Desconectado del Host.");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -=
            OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback -=
            OnClientDisconnected;
    }
}