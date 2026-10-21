
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class RelayPingMonitor : MonoBehaviour
{
    private UnityTransport transport;
    private float pingTimer;

    private void Update()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (transport == null)
        {
            transport = NetworkManager.Singleton
                .GetComponent<UnityTransport>();
        }

        if (transport == null || !NetworkManager.Singleton.IsListening)
            return;

        pingTimer += Time.deltaTime;

        if (pingTimer < 1f)
            return;

        pingTimer = 0f;

        if (NetworkManager.Singleton.IsServer)
        {
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId == NetworkManager.ServerClientId)
                    continue;

                ulong rtt = transport.GetCurrentRtt(clientId);
                Debug.Log($"[HOST] RTT hacia cliente {clientId}: {rtt} ms");
            }
        }
        else
        {
            ulong rtt = transport.GetCurrentRtt(
                NetworkManager.ServerClientId);

            Debug.Log($"[CLIENTE] RTT hacia el host: {rtt} ms");
        }
    }
}
