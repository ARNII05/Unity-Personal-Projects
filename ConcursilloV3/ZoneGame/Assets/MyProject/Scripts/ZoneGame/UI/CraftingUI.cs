using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    private GameObject craftingUI;

    private Image childItemImg, coreItemImg;
    private TextMeshProUGUI itemTextCount;
    private GameObject boxBtn, childrenObj;
    private Button boxItemBtn;
    
    private Player player;

    private const string itemsBasePath = "Prefabs/ItemsImg/";

    void Start()
    {
        craftingUI = transform.GetChild(0).gameObject;
        craftingUI.SetActive(false);
        
        boxBtn = craftingUI.transform.Find("CoreItemBtn").gameObject;
        boxItemBtn = boxBtn.GetComponent<Button>();
        childrenObj = craftingUI.transform.Find("ChildItem").gameObject;
        itemTextCount = childrenObj.transform.GetComponentInChildren<TextMeshProUGUI>();
        childItemImg = childrenObj.transform.Find("ItemImg").GetComponent<Image>();
        coreItemImg = craftingUI.transform.Find("CoreItem").Find("ItemIcon").GetComponent<Image>();
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
        UpdateCanvasGroup(player.Role.Value == PlayerRole.Gardener
            ? player.craftingSystem.CanCraftBouquet()
            : player.craftingSystem.CanCraftLog());
    }

    private void UpdateUI()
    {
        UpdateItemTextCount(player.Role.Value);

        bool canBuildItem = player.Role.Value == PlayerRole.Gardener 
            ? player.craftingSystem.CanCraftBouquet() 
            : player.craftingSystem.CanCraftLog();

        UpdateCanvasGroup(canBuildItem);
    }

    private void UpdateCanvasGroup(bool haveNecessaryItems = false)
    {
        CanvasGroup canvasGroup = boxBtn.transform.GetComponent<CanvasGroup>();

        canvasGroup.alpha = haveNecessaryItems ? 1 : 0.4f;
        canvasGroup.interactable = haveNecessaryItems;
        canvasGroup.blocksRaycasts = haveNecessaryItems;
    }

    private void SetUI()
    {
        PlayerRole playerRole = player.Role.Value;

        UpdateItemTextCount(playerRole);

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
    }

    private void UpdateItemTextCount(PlayerRole playerRole)
    {
        ItemType itemType = playerRole == PlayerRole.Gardener ? ItemType.Flower : ItemType.Branch;

        int maxItemAmount = playerRole == PlayerRole.Gardener
            ? CraftingSystem.flowersForBouquet
            : CraftingSystem.branchesForLog;

        int itemAmount = player.inventory.GetAmount(itemType);


        itemTextCount.text = $"{itemAmount}/{maxItemAmount}";
    }

    public void OpenCraftingUI()
    {
        craftingUI.SetActive(true);
    }

    public void CloseCraftingUI()
    {
        craftingUI.SetActive(false);
    }
}
