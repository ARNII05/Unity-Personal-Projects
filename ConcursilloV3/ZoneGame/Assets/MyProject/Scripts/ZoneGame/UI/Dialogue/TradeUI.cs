using System.Security.Cryptography.X509Certificates;
using UnityEngine;

[RequireComponent(typeof(Trade))]
public class TradeUI : MonoBehaviour
{
    [SerializeField] private GameObject tradeDialogue;

    public static TradeUI Instance { get; private set; }

    private Player player;

    private Trade trade;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        trade = GetComponent<Trade>();
    }

    void Start()
    {
        tradeDialogue.SetActive(false);
    }

    public void Init(Player player)
    {
        this.player = player;
        trade.Init(player);
    }

    public void OpenUI()
    {
        tradeDialogue.SetActive(true);

        player.SetNewStateRpc(PlayerState.TradeUI);
    }

    public void CloseUI()
    {
        tradeDialogue.SetActive(false);

        trade.ClosePanel();

        player.SetNewStateRpc(PlayerState.Normal);
    }

    public void OpenTradeObjectsWithCoins()
    {
        tradeDialogue.SetActive(false);
        trade.OpenTradeObjectsWithCoins();
    }

    public void OpenTradeObjects()
    {
        tradeDialogue.SetActive(false);
        trade.OpenTradeObjects();
    }
}
