using UnityEngine;

public class Trade : MonoBehaviour
{
    [SerializeField] private GameObject tradePanel;

    private GameObject tradeObject;
    private GameObject tradeObjectWithCoins;

    public static TradeUI Instance { get; private set; }

    private Player player;

    private void Awake()
    {
        tradeObject = tradePanel.transform.Find("TradeObjects").gameObject;
        tradeObjectWithCoins = tradePanel.transform.Find("TradeObjectsWithCoins").gameObject;
    }

    void Start()
    {
        tradePanel.SetActive(false);
        tradeObject.SetActive(false);
        tradeObjectWithCoins.SetActive(false);
    }

    public void Init(Player player)
    {
        this.player = player;
    }

    public void OpenTradeObjects()
    {
        tradePanel.SetActive(true);
        tradeObject.SetActive(true);
    }

    public void OpenTradeObjectsWithCoins()
    {
        tradePanel.SetActive(true);
        tradeObjectWithCoins.SetActive(true);
    }
    
    public void ClosePanel()
    {
        tradePanel.SetActive(false);
        tradeObjectWithCoins.SetActive(false);
        tradeObject.SetActive(false);
    }
}
