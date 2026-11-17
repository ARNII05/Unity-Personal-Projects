using UnityEngine;
using TMPro;

public class MissingLogsUI : MonoBehaviour
{
    public static MissingLogsUI Instance { get; private set; }

    private GameObject missingLogsUI, bridged, notBridged;

    private Player player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        missingLogsUI = transform.Find("MissingLogsUI").gameObject;
        bridged = missingLogsUI.transform.Find("Bridged").gameObject;
        notBridged = missingLogsUI.transform.Find("NotBridged").gameObject;
        
        missingLogsUI.SetActive(false);
        bridged.SetActive(false);
        notBridged.SetActive(false);
    }

    public void Init(Player player)
    {
        this.player = player;
    }

    public void ShowMissingLogs(int missingLogs, int playerLogs, InteractResult interactResult)
    {
        player.SetNewStateRpc(PlayerState.MissingLogsUI);

        missingLogsUI.SetActive(true);
        notBridged.SetActive(false);
        bridged.SetActive(false);

        if (interactResult == InteractResult.IsBridged)
        {
            bridged.SetActive(true);
            return;
        }

        notBridged.SetActive(true);

        int necessaryLogs = CraftingSystem.logsForBridge;

        TMP_Text mainText = notBridged.transform.Find("MainText").GetComponent<TMP_Text>();
        GameObject itemBox = notBridged.transform.Find("ItemBox").gameObject;
        TMP_Text itemCount = itemBox.transform.Find("ItemCount").GetComponent<TMP_Text>();

        itemCount.text = $"{playerLogs} / {necessaryLogs}";
        
        mainText.text = missingLogs == 1
            ? "Te falta 1 tronco para construir el puente"
            : $"Te faltan {missingLogs} troncos para construir el puente";
    }

    public void Close()
    {
        player.SetNewStateRpc(PlayerState.Normal);
        missingLogsUI.SetActive(false);
    }
}
