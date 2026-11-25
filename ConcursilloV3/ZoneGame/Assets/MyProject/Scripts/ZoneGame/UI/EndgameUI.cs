using TMPro;
using Unity.Netcode;
using UnityEngine;

public class EndgameUI : NetworkBehaviour
{
    public static EndgameUI Instance { get; private set; }

    private GameObject endgameUI, objectsPanel, statsGrid;

    private TMP_Text gardenerName, builderName, timeUsed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        endgameUI = transform.Find("EndgameUI").gameObject;
        objectsPanel = endgameUI.transform.Find("ObjectsPanel").gameObject;
        statsGrid = objectsPanel.transform.Find("StatsGrid").gameObject;

        gardenerName = objectsPanel.transform.Find("GardenerZell").Find("PlayerName").GetComponent<TMP_Text>();
        builderName = objectsPanel.transform.Find("BuilderZell").Find("PlayerName").GetComponent<TMP_Text>();
        timeUsed = objectsPanel.transform.Find("TimeUsed").GetComponent<TMP_Text>();

        endgameUI.SetActive(false);
    }

    public void ShowStats(int[] gardenerData, int[] builderData, float gameTime)
    {
        endgameUI.SetActive(true);

        Player player = MapManager.instance.Player1;
        Player player2 = MapManager.instance.Player2;

        gardenerName.text = player.Role.Value == PlayerRole.Gardener
            ? player.name
            : player2.name;

        builderName.text = player.Role.Value == PlayerRole.Builder
            ? player.name
            : player2.name;

        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);

        timeUsed.text = $"{minutes:00}:{seconds:00}";

        for (int i = 0; i < gardenerData.Length &&
                i < builderData.Length &&
                i < statsGrid.transform.childCount; i++)
        { 
            GameObject zell = statsGrid.transform.GetChild(i).gameObject;
            TMP_Text gardenerText = zell.transform.Find("GardenerText").GetComponent<TMP_Text>();
            TMP_Text builderText = zell.transform.Find("BuilderText").GetComponent<TMP_Text>();

            gardenerText.text = $"{gardenerData[i]}";
            builderText.text = $"{builderData[i]}";
        }
    }
    
    public void ApplicationQuit()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        Application.Quit();
    }

    public void GoToMenu()
    {
        RequestReturnToMenuRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestReturnToMenuRpc()
    {
        ShowWinPanelInMenuClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ShowWinPanelInMenuClientRpc()
    {
        string winMessage = "¡Enhorabuena!\n\n" +
                    "¡Habéis completado Hello Grandma!\n\n" +
                    "Muchas gracias por jugar. ¡Espero que hayáis disfrutado de la aventura!";

        PersistentNetworkManager.Instance.ReturnToMainMenu(winMessage, isErrorMsg: false);
    }
}
