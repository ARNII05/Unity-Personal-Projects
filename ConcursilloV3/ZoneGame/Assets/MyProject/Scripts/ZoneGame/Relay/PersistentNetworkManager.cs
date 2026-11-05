using UnityEngine;
using Unity.Netcode;

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
}