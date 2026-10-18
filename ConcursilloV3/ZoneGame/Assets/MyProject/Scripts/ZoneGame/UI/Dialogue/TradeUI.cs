using System.Security.Cryptography.X509Certificates;
using UnityEngine;

[RequireComponent(typeof(Trade))]
public class TradeUI : MonoBehaviour
{
    [SerializeField] private GameObject tradeUI;

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
        tradeUI.SetActive(false);
    }


    public void Init(Player player)
    {
        this.player = player;
        trade.Init(player);
    }

    public void OpenUI()
    {
        tradeUI.SetActive(true);

        player.State = PlayerState.TradeUI;
    }

    public void CloseUI()
    {
        tradeUI.SetActive(false);

        player.State = PlayerState.Normal;
    }

    private void OpenTradeObjects()
    {

    }

    private void OpenTradeObjectsWithCoins()
    {

    }
}
