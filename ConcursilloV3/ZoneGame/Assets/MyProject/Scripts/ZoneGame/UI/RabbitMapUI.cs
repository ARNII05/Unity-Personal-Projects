using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RabbitMapUI : MonoBehaviour
{
    public static RabbitMapUI Instance { get; private set; }

    [SerializeField] private GameObject rabbitMap;
    [SerializeField] private Sprite questionSprite, UserSprite;
    
    private GameObject grid, zoneInfo, zoneZellPrefab, lockObj;

    private Image zoneImage;

    private Button tpButton;

    private TextMeshProUGUI zoneName, zoneState, chestState;

    private ZoneData[,] map;

    private Player player;

    private Vector2Int zoneToTp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        rabbitMap.SetActive(false);
        grid = rabbitMap.transform.Find("Grid").gameObject;
        
        zoneInfo = rabbitMap.transform.Find("ZoneInfo").gameObject;
        zoneImage = zoneInfo.transform.Find("ZoneImage").GetComponent<Image>();
        zoneName = zoneInfo.transform.Find("ZoneName").GetComponent<TextMeshProUGUI>();
        zoneState = zoneInfo.transform.Find("ZoneState").GetComponent<TextMeshProUGUI>();
        chestState = zoneInfo.transform.Find("ChestState").GetComponent<TextMeshProUGUI>();
        tpButton = zoneInfo.transform.Find("TpButton").GetComponent<Button>();
        lockObj = tpButton.transform.Find("Lock").gameObject;

        zoneZellPrefab = Resources.Load<GameObject>("Prefabs/Objects/ZoneZell");
    }

    public void ToggleRabbitMap()
    {
        if (rabbitMap.activeSelf)
            CloseMap();
        else
            OpenMap();
    }

    private void OpenMap()
    {
        player.State = PlayerState.UsingRabbitMap;

                SetZoneInfo(false);
        SetZoneIcons();

        rabbitMap.SetActive(true);
    }

    private void CloseMap()
    {
        player.State = PlayerState.Normal;
        
        rabbitMap.SetActive(false);
    }

    public void TpToZone()
    {
        CloseMap();
        
        UIManager.Instance.PlayZoneTransition(
            () =>
            {
                MapManager.Instance.TeleportPlayer(
                    player,
                    zoneToTp
                );
            },
            player
        );
    }

    private void SetZoneInfo(bool parseName = true, string name = "")
    {
        Vector2Int zonePos = parseName ? GetZonePosition(name) : player.NetworkMapPos.Value;

        ZoneData zone = map[zonePos.y, zonePos.x];

        bool zoneDiscovered = player.discoveredZones.Contains(zonePos);

        InitTpButton(zoneDiscovered, zonePos == player.NetworkMapPos.Value, 
            zone.type == ZoneType.HorizontalRiver || zone.type == ZoneType.VerticalRiver, 
            zonePos);

        zoneImage.sprite = zoneDiscovered ? map[zonePos.y, zonePos.x].icon : questionSprite;
        
        zoneName.text = zoneDiscovered switch
        {
            true => $"{zone.GetName()} ({zonePos.y}, {zonePos.x})",
            _ => $"Zona desconocida ({zonePos.y}, {zonePos.x})"
        };
        zoneState.text = $"Estado: {(zoneDiscovered ? "Descubierta" : "Sin descubrir")}";
        chestState.text = $"Cofre: {(zone.chestOpened ? "Abierto" : "Sin abrir")}";
    }

    private void InitTpButton(bool isDiscovered, bool isSameZoneAsPlayer, bool isRiver, Vector2Int zonePos)
    {
        bool canInteract = isDiscovered && !isSameZoneAsPlayer && !isRiver;

        lockObj.SetActive(!canInteract);
        tpButton.interactable = canInteract;

        CanvasGroup canvasGroup = tpButton.transform.Find("CanvasGroup").GetComponent<CanvasGroup>();

        canvasGroup.alpha = canInteract ? 1 : 0.4f;

        if (canInteract) zoneToTp = zonePos;
    }

    private void SetZoneIcons()
    {
        foreach (Transform child in grid.transform)
        {
            Vector2Int zonePos = GetZonePosition(child.name);
            child.GetComponent<Image>().sprite = GetZoneIcon(zonePos);
        }
    }

    private Vector2Int GetZonePosition(string zoneName)
    {
        string[] parts = zoneName.Split("_");

        int y = int.Parse(parts[1]);
        int x = int.Parse(parts[2]);

        return new Vector2Int(x, y);
    }

    private Sprite GetZoneIcon(Vector2Int zonePos)
    {
        if (player.NetworkMapPos.Value == zonePos)
            return UserSprite;

        return player.discoveredZones.TryGetValue(zonePos, out var pos) ? map[pos.y, pos.x].icon : questionSprite;
    }

    public void InitMap(Player player)
    {
        map = MapManager.Instance.map;

        for (int y = map.GetLength(0) - 1; y >= 0; y--)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                GameObject actualZone =
                    Instantiate(zoneZellPrefab, grid.transform);

                Button actualZoneBtn = actualZone.GetComponent<Button>();

                actualZone.name = $"Zone_{y}_{x}";

                actualZoneBtn.onClick.AddListener(
                    () => SetZoneInfo(true, actualZone.name)
                );
            }
        }

        this.player = player;
    }
}
