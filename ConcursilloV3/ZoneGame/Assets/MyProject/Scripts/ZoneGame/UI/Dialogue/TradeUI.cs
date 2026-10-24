using System.Security.Cryptography.X509Certificates;
using UnityEngine;

[RequireComponent(typeof(TradeObjects))]
public class TradeUI : MonoBehaviour
{
    [SerializeField] private GameObject tradeDialogue;

    public static TradeUI Instance { get; private set; }

    private Player player;

    public TradeObjects tradeObjects;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        tradeObjects = GetComponent<TradeObjects>();
    }

    void Start()
    {
        tradeDialogue.SetActive(false);
    }

    public void Init(Player player)
    {
        this.player = player;
        tradeObjects.Init(player);
    }

    public void OpenUI()
    {
        tradeDialogue.SetActive(true);

        player.SetNewStateRpc(PlayerState.TradeUI);
    }

    public void CloseUI()
    {
        tradeDialogue.SetActive(false);

        tradeObjects.ClosePanel();

        player.SetNewStateRpc(PlayerState.Normal);
    }

    public void OpenTradeObjects(int type)
    {
        tradeDialogue.SetActive(false);
        tradeObjects.OpenTrade(type == 0 ? TradeMode.Buy : TradeMode.Sell);
    }
}