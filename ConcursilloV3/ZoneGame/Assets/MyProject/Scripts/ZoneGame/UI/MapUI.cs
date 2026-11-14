using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class MapUI : MonoBehaviour
{
    public static MapUI Instance { get; private set; }

    private GameObject mapUI;
    
    private GameObject mapZones, grid;
    private TextMeshProUGUI p1Coordinates, p2Coordinates, p1Name, p2Name,
        grandmaHouseCoordinates, wolfDenCoordinates, momHouseCoordinates;

    private Player mapPlayer;
    private Player otherPlayer;

    private ZoneData[,] map;

    private ZoneTooltipUI zoneTooltipUI;

    private const string gardenerRolColor = "#356344";
    private const string builderRolColor = "#754327";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        zoneTooltipUI = GameObject.Find("ZoneTooltipPanel").GetComponent<ZoneTooltipUI>();
    }

    private void Start()
    {
        mapUI = transform.Find("MapUI").gameObject;
        mapZones = mapUI.transform.Find("MapZones").gameObject;
        grid = mapUI.transform.Find("CoordinatesGrid").gameObject;
        
        p1Coordinates = mapUI.transform.Find("P1Info").Find("Coordinates").GetComponent<TextMeshProUGUI>();
        p2Coordinates = mapUI.transform.Find("P2Info").Find("Coordinates").GetComponent<TextMeshProUGUI>();

        p1Name = mapUI.transform.Find("P1Info").Find("PlayerName").GetComponent<TextMeshProUGUI>();
        p2Name = mapUI.transform.Find("P2Info").Find("PlayerName").GetComponent<TextMeshProUGUI>();

        grandmaHouseCoordinates = grid.transform.Find("GrandmaZell").Find("Coordinates").GetComponent<TextMeshProUGUI>();
        wolfDenCoordinates = grid.transform.Find("WolfZell").Find("Coordinates").GetComponent<TextMeshProUGUI>();
        momHouseCoordinates = grid.transform.Find("MomZell").Find("Coordinates").GetComponent<TextMeshProUGUI>();

        mapUI.SetActive(false);
    }

    public void Init(Player player)
    {
        mapPlayer = player;
        otherPlayer = MapManager.Instance.GetOtherPlayer(player);

        mapPlayer.NetworkMapPos.OnValueChanged += OnPlayerPositionChanged;
        otherPlayer.NetworkMapPos.OnValueChanged += OnPlayerPositionChanged;

        StartCoroutine(WaitForMapReady());
        StartCoroutine(WaitForPlayerRols());
    }

    private IEnumerator WaitForMapReady()
    {
        yield return new WaitUntil(() => MapManager.Instance.IsMapReady);

        UpdateMap();
    }

    private IEnumerator WaitForPlayerRols()
    {
        yield return new WaitUntil(() => MapManager.Instance.Player1 != null 
            && MapManager.Instance.Player2 != null &&
            MapManager.Instance.Player1.Role.Value != PlayerRole.None &&
            MapManager.Instance.Player2.Role.Value != PlayerRole.None);

        Player player1 = MapManager.Instance.Player1;
        Player player2 = MapManager.Instance.Player2;

        PlayerRole p1Role = player1.Role.Value;
        PlayerRole p2Role = player2.Role.Value;

        Color p1Color = p1Role == PlayerRole.Gardener ?
            HexToColor(gardenerRolColor)
            : HexToColor(builderRolColor);
        Color p2Color = p2Role == PlayerRole.Gardener ?
            HexToColor(gardenerRolColor)
            : HexToColor(builderRolColor);

        p1Name.text = player1.name;
        p2Name.text = player2.name;

        p1Name.color = p1Color;
        p2Name.color = p2Color;

        p1Coordinates.color = p1Color;
        p2Coordinates.color = p2Color;
    }

    private Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }

    public void ToggleMap()
    {
        if (mapUI.activeSelf)
            OnClose();
        else
            OnOpen();
    }

    private void OnOpen()
    {
        mapPlayer.SetNewStateRpc(PlayerState.UsingMap);

        mapUI.SetActive(true);

        StartCoroutine(HideTooltipNextFrame());
    }

    private IEnumerator HideTooltipNextFrame()
    {
        yield return null;

        HideZoneTooltip();
    }

    private void OnPlayerPositionChanged(
        Vector2Int oldPos,
        Vector2Int newPos)
    {
        if (mapPlayer == null ||
            MapManager.Instance == null ||
            !MapManager.Instance.IsMapReady)
            return;

        UpdateMap();
    }

    private void OnClose()
    {
		mapPlayer.SetNewStateRpc(PlayerState.Normal);

        HideZoneTooltip();

        mapUI.SetActive(false);
    }

    public void UpdateMap()
    {
        map = MapManager.Instance.map;

        UpdateCoordinates();

        UpdateZoneIcons();
    }

    private void UpdateCoordinates()
    {
        Vector2Int playerPos = mapPlayer.NetworkMapPos.Value;
        Vector2Int otherPlayerPos = otherPlayer.NetworkMapPos.Value;
        Vector2Int grandmaHousePos = MapManager.Instance.grandmaPos;
        Vector2Int wolfDenPos = MapManager.Instance.wolfDenPos;

        Vector2Int startPos = MapManager.Instance.startPos;

        p1Coordinates.text = $"Y: {playerPos.y + 1} X: {playerPos.x + 1}";
        p2Coordinates.text = $"Y: {otherPlayerPos.y + 1} X: {otherPlayerPos.x + 1}";

        grandmaHouseCoordinates.text = $"Y: {grandmaHousePos.y + 1} X: {grandmaHousePos.x + 1}";
        wolfDenCoordinates.text = $"Y: {wolfDenPos.y + 1} X: {wolfDenPos.x + 1}";
        momHouseCoordinates.text = $"Y: {startPos.y + 1} X: {startPos.x + 1}";
    }

    private void UpdateZoneIcons()
    {
        GameObject actualZone = mapZones.transform.Find("ActualZone").gameObject;
        GameObject northZone = mapZones.transform.Find("NorthZone").gameObject;
        GameObject southZone = mapZones.transform.Find("SouthZone").gameObject;
        GameObject eastZone = mapZones.transform.Find("EastZone").gameObject;
        GameObject westZone = mapZones.transform.Find("WestZone").gameObject;

        Vector2Int playerPos = mapPlayer.NetworkMapPos.Value;

        actualZone.GetComponent<Image>().sprite = map[playerPos.y, playerPos.x].icon;
        northZone.GetComponent<Image>().sprite = GetZoneIcon(Direction.North, map, playerPos);
        southZone.GetComponent<Image>().sprite = GetZoneIcon(Direction.South, map, playerPos);
        eastZone.GetComponent<Image>().sprite = GetZoneIcon(Direction.East, map, playerPos);
        westZone.GetComponent<Image>().sprite = GetZoneIcon(Direction.West, map, playerPos);
    }

    private Sprite GetZoneIcon(Direction direction, ZoneData[,] map, Vector2Int playerPos)
    {
        Vector2Int targetPos = direction switch
        {
            Direction.North => playerPos + Vector2Int.up,
            Direction.South => playerPos + Vector2Int.down,
            Direction.East => playerPos + Vector2Int.right,
            Direction.West => playerPos + Vector2Int.left,
            _ => playerPos
        };

        if (targetPos.x < 0 || targetPos.x >= map.GetLength(1) ||
            targetPos.y < 0 || targetPos.y >= map.GetLength(0))
        {
            return Resources.Load<Sprite>("Prefabs/ZoneIcons/Cross");
        }

        ZoneData zone = map[targetPos.y, targetPos.x];

        return zone != null ? zone.icon : Resources.Load<Sprite>("Prefabs/ZoneIcons/Cross");
    }

    public void ShowZoneTooltip(Direction direction, RectTransform rectTransform)
    {
        Vector2Int playerPos = mapPlayer.NetworkMapPos.Value;

        Vector2Int targetPos = direction switch
        {
            Direction.North => playerPos + Vector2Int.up,
            Direction.South => playerPos + Vector2Int.down,
            Direction.East => playerPos + Vector2Int.right,
            Direction.West => playerPos + Vector2Int.left,
            _ => playerPos
        };

        bool isOutOfBounds = targetPos.x < 0 || targetPos.x >= map.GetLength(1) ||
            targetPos.y < 0 || targetPos.y >= map.GetLength(0);

        ZoneData zone = isOutOfBounds ? null : map[targetPos.y, targetPos.x];

        zoneTooltipUI.ShowZoneTooltip(zone, rectTransform);
    }

    public void HideZoneTooltip()
    {
        zoneTooltipUI.HideTooltip();
    }
}