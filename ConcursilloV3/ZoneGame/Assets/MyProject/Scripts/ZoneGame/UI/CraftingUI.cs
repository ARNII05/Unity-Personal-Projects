using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    public static CraftingUI Instance { get; private set; }

    [SerializeField] private GameObject craftingUI;

    private Image childItemImg, coreItemImg;
    private TextMeshProUGUI itemTextCount;
    private GameObject boxBtn, lockObj, childrenObj;
    private Button boxItemBtn;
    
    private Player player;

    private const string itemsBasePath = "Prefabs/ItemsImg/";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        boxBtn = craftingUI.transform.Find("CoreItemBtn").gameObject;
        boxItemBtn = boxBtn.GetComponent<Button>();
        lockObj = boxBtn.transform.Find("Lock").gameObject;
        childrenObj = craftingUI.transform.Find("ChildItem").gameObject;
        itemTextCount = childrenObj.transform.GetComponentInChildren<TextMeshProUGUI>();
        childItemImg = childrenObj.transform.Find("ItemImg").GetComponent<Image>();
        coreItemImg = boxBtn.transform.GetChild(0).Find("ItemIcon").GetComponent<Image>();
    }

    public void SetPlayer(Player player)
    {
        this.player = player;
        player.inventory.OnInventoryChanged += UpdateUI;
    }

    public void Init()
    {
        if (player.Role.Value == PlayerRole.Gardener)
        {
            boxItemBtn.onClick.AddListener(player.craftingSystem.CraftBouquet);
        }
        else
        {
            boxItemBtn.onClick.AddListener(player.craftingSystem.CraftLog);
        }
        SetUI();
        UpdateCanvasGroup();
    }

    private void UpdateUI()
    {
        ItemType itemType = player.Role.Value == PlayerRole.Gardener ? ItemType.Flower : ItemType.Branch;

        int itemAmount = player.inventory.GetItemAmount(itemType);

        int maxItemAmount = CraftingSystem.flowersForBouquet;

        bool canBuildItem = player.Role.Value == PlayerRole.Gardener 
            ? player.craftingSystem.CanCraftBouquet() 
            : player.craftingSystem.CanCraftLog();

        UpdateCanvasGroup(canBuildItem);

        itemTextCount.text = $"{itemAmount}/{maxItemAmount}";
    }

    private void UpdateCanvasGroup(bool haveNecessaryItems = false)
    {
        CanvasGroup canvasGroup = boxBtn.transform.Find("CanvasGroup").GetComponent<CanvasGroup>();

        lockObj.SetActive(!haveNecessaryItems);

        canvasGroup.alpha = haveNecessaryItems ? 1 : 0.4f;
        canvasGroup.interactable = haveNecessaryItems;
        canvasGroup.blocksRaycasts = haveNecessaryItems;
    }

    private void SetUI()
    {
        PlayerRole playerRole = player.Role.Value;
        int maxItemAmount = CraftingSystem.flowersForBouquet;

        childItemImg.sprite = (playerRole) switch
        {
            (PlayerRole.Gardener) => Resources.Load<Sprite>(itemsBasePath + "Flower"),
            _ => Resources.Load<Sprite>(itemsBasePath + "Branch"),
        };

        coreItemImg.sprite = (playerRole) switch
        {
            (PlayerRole.Gardener) => Resources.Load<Sprite>(itemsBasePath + "Bouquet"),
            _ => Resources.Load<Sprite>(itemsBasePath + "Log"),
        };

        itemTextCount.text = $"5/{maxItemAmount}";
    }
}
