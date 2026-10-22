using System.Security.Cryptography.X509Certificates;
using UnityEngine;

[RequireComponent(typeof(TradeObjectsWithCoins))]
public class TradeUI : MonoBehaviour
{
    [SerializeField] private GameObject tradeDialogue;

    public static TradeUI Instance { get; private set; }

    private Player player;

    private TradeObjectsWithCoins tradeWithCoins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        tradeWithCoins = GetComponent<TradeObjectsWithCoins>();
    }

    void Start()
    {
        tradeDialogue.SetActive(false);
    }

    public void Init(Player player)
    {
        this.player = player;
        tradeWithCoins.Init(player);
    }

    public void OpenUI()
    {
        tradeDialogue.SetActive(true);

        player.SetNewStateRpc(PlayerState.TradeUI);
    }

    public void CloseUI()
    {
        tradeDialogue.SetActive(false);

        tradeWithCoins.ClosePanel();

        player.SetNewStateRpc(PlayerState.Normal);
    }

    public void OpenTradeObjectsWithCoins()
    {
        tradeDialogue.SetActive(false);
        tradeWithCoins.OpenTrade();
    }

    public void OpenTradeObjects()
    {
        tradeDialogue.SetActive(false);
    }
}
