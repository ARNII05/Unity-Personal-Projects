using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RabbitMapUI : MonoBehaviour
{
    private GameObject rabbitUI;
    [SerializeField] private Sprite questionSprite, userSprite;
    
    private GameObject grid, zoneInfo, zoneZellPrefab, lockObj;

    private Image zoneImage;

    private Button tpButton;

    private TextMeshProUGUI zoneName, zoneDesc, chestState, rabbitCd;

    private ZoneData[,] map;

    private Player player;

    private Vector2Int zoneToTp;

    private GameObject selectedZone = null;

    private void Start()
    {
        rabbitUI = transform.Find("RabbitUI").gameObject;

        grid = rabbitUI.transform.Find("Grid").gameObject;
        
        zoneInfo = rabbitUI.transform.Find("ZoneInfo").gameObject;
        zoneImage = zoneInfo.transform.Find("ZoneImage").GetComponent<Image>();
        zoneName = zoneInfo.transform.Find("ZoneName").GetComponent<TextMeshProUGUI>();
        zoneDesc = zoneInfo.transform.Find("ZoneDesc").GetComponent<TextMeshProUGUI>();
        chestState = zoneInfo.transform.Find("ChestState").GetComponent<TextMeshProUGUI>();
        rabbitCd = zoneInfo.transform.Find("RabbitCd").GetComponent<TextMeshProUGUI>();
        tpButton = zoneInfo.transform.Find("TpButton").GetComponent<Button>();
        lockObj = tpButton.transform.Find("Lock").gameObject;

        zoneZellPrefab = Resources.Load<GameObject>("Prefabs/Objects/ZoneZell");

        rabbitUI.SetActive(false);
    }

    private void Update()
    {
        if (!rabbitUI.activeSelf || !selectedZone)
            return;

        float remaining = player.GetRabbitTeleportRemainingTime();

        bool IsInSameZone = player.NetworkMapPos.Value == GetZonePosition(selectedZone.name);

        rabbitCd.text = (IsInSameZone, remaining > 0) switch
        {
            (true, _) => "Teletransporte no disponible",
            (false, true) => $"Cooldown: {Mathf.CeilToInt(remaining)}s",
            _ => "Teletransporte disponible"
        };

        rabbitCd.color = (IsInSameZone, remaining > 0) switch
        {
            (true, _) => new Color32(255, 0, 0, 255),
            (false, true) => new Color32(255, 0, 0, 255),
            _ => Color.green
        };

        if (remaining <= 0)
            UpdateTpButton();
    }

    private void UpdateTpButton()
    {
        Vector2Int zonePos = GetZonePosition(selectedZone.name);

        Vector2Int playerPos = player.NetworkMapPos.Value;

        InitTpButton(zonePos == playerPos,
            player.CanRabbitTeleport(),
            zonePos);
    }

    public void Init(Player player)
    {
        map = MapManager.Instance.map;
        this.player = player;

        InitMap();
    }

    public void ToggleRabbitMap()
    {
        if (rabbitUI.activeSelf)
            CloseMap();
        else
            OpenMap();
    }

    private void OpenMap()
    {
        player.SetState(PlayerState.RabbitMapUI);

        UpdateMap();
        SetZoneInfo(false);
        SetZoneIcons();

        rabbitUI.SetActive(true);
    }

    private void CloseMap()
    {
        player.SetState(PlayerState.Normal);

        rabbitUI.SetActive(false);
    }

    public void UpdateZoneInfo()
    {
        if (!rabbitUI.activeSelf)
            return;

        chestState.text = $"Cofre abierto";
    }

    public void TpToZone()
    {
        if (!player.CanRabbitTeleport())
            return;

        player.StartRabbitTeleportCooldown();

        CloseMap();

        player.ReproduceSFXSoundClientRpc(SFXType.Teleport);

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

        SwapBackGroundColor(zonePos);
        
        InitTpButton(zonePos == player.NetworkMapPos.Value, 
            player.CanRabbitTeleport(),
            zonePos);

        zoneImage.sprite = map[zonePos.y, zonePos.x].icon;

        zoneName.text = $"{zone.name} ({zonePos.y + 1}, {zonePos.x + 1})";

        zoneDesc.text = ZoneVault.GetZoneDescription(zone.type);
        chestState.text = $"Cofre {(zone.chestOpened ? "abierto" : "sin abrir")}";
    }

    private void SwapBackGroundColor(Vector2Int zonePos)
    {
        GameObject zone = grid.transform.Find($"Zone_{zonePos.y}_{zonePos.x}").gameObject;

        if (selectedZone != null)
        {
            Image selectedZoneBackground = selectedZone.transform.Find("Background").GetComponent<Image>();
            
            Color color = selectedZoneBackground.color;
            color.a = 0f;
            selectedZoneBackground.color = color;
            selectedZone.transform.localScale = Vector3.one;
        }

        Image actualZoneImg = zone.transform.Find("Background").GetComponent<Image>();
        
        Color actualColor = actualZoneImg.color;
        actualColor.a = 0.45f;
        actualZoneImg.color = actualColor;
        
        zone.transform.localScale = Vector3.one * 1.3f;

        selectedZone = zone;
    }

    private void InitTpButton(bool isSameZoneAsPlayer, bool rabbitAllowsTp, Vector2Int zonePos)
    {
        bool canInteract = rabbitAllowsTp && !isSameZoneAsPlayer;

        lockObj.SetActive(!canInteract);
        tpButton.interactable = canInteract;

        CanvasGroup canvasGroup = tpButton.transform.Find("CanvasGroup").GetComponent<CanvasGroup>();

        canvasGroup.alpha = canInteract ? 1 : 0.4f;

        zoneToTp = canInteract ? zonePos : zoneToTp;
    }

    private void SetZoneIcons()
    {
        foreach (Transform child in grid.transform)
        {
            Vector2Int zonePos = GetZonePosition(child.name);
            child.Find("ZoneIcon").GetComponent<Image>().sprite = GetZoneIcon(zonePos);
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
        return player.NetworkMapPos.Value == zonePos ? userSprite : map[zonePos.y, zonePos.x].icon;
    }

    private void InitMap()
    {
        selectedZone = null;

        foreach (Transform child in grid.transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        for (int y = map.GetLength(0) - 1; y >= 0; y--)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                GameObject actualZone =
                    Instantiate(zoneZellPrefab, grid.transform);

                Image zoneImg = actualZone.transform.Find("Background").GetComponent<Image>();

                Color color = zoneImg.color;
                color.a = 0f;

                zoneImg.color = color;

                Button actualZoneBtn = actualZone.GetComponent<Button>();

                actualZone.name = $"Zone_{y}_{x}";

                actualZoneBtn.onClick.AddListener(
                    () => SetZoneInfo(true, actualZone.name)
                );

                bool isRiver = map[y, x].type == ZoneType.HorizontalRiver ||
                    map[y, x].type == ZoneType.VerticalRiver;

                actualZone.SetActive(
                    player.discoveredZones.Contains(new Vector2Int(x, y)) &&
                    !isRiver);
            }
        }
    }

    private void UpdateMap()
    {
        foreach (Transform child in grid.transform)
        {
            Vector2Int zonePos = GetZonePosition(child.name);

            bool isRiver = map[zonePos.y, zonePos.x].type is
                ZoneType.HorizontalRiver or ZoneType.VerticalRiver;

            child.gameObject.SetActive(
                player.discoveredZones.Contains(zonePos) && !isRiver
            );
        }
    }

    private enum RabbitCdType
    {
        TpNotAvailable,
        IsInSameZone,
        TpAvailable
    }
}
