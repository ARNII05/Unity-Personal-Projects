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
    
    private GameObject mapZones;
    private TextMeshProUGUI p1Coordinates;
    private TextMeshProUGUI p2Coordinates;
    private TextMeshProUGUI grandmaHouseCoordinates;
    private TextMeshProUGUI wolfDenCoordinates;

    private Player mapPlayer;
    private Player otherPlayer;

    private ZoneData[,] map;

    private ZoneTooltipUI zoneTooltipUI;

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
        p1Coordinates = mapUI.transform.Find("P1Coordinates").GetComponent<TextMeshProUGUI>();
        p2Coordinates = mapUI.transform.Find("P2Coordinates").GetComponent<TextMeshProUGUI>();
        grandmaHouseCoordinates = mapUI.transform.Find("GrandmaHouseCoordinates").GetComponent<TextMeshProUGUI>();
        wolfDenCoordinates = mapUI.transform.Find("WolfDenCoordinates").GetComponent<TextMeshProUGUI>();
        mapUI.SetActive(false);
    }

    public void Init(Player player)
    {
        mapPlayer = player;
        otherPlayer = MapManager.Instance.GetOtherPlayer(player);

        mapPlayer.NetworkMapPos.OnValueChanged += OnPlayerPositionChanged;
        otherPlayer.NetworkMapPos.OnValueChanged += OnPlayerPositionChanged;

        StartCoroutine(WaitForMapReady());
    }

    private IEnumerator WaitForMapReady()
    {
        yield return new WaitUntil(() => MapManager.Instance.IsMapReady);

        UpdateMap();
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
    }

    private void OnPlayerPositionChanged(
        Vector2Int oldPos,
        Vector2Int newPos)
    {
        if (mapPlayer == null)
            return;

        UpdateMap();
    }

    private void OnClose()
    {
		mapPlayer.SetNewStateRpc(PlayerState.Normal);

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

        p1Coordinates.text = $"{mapPlayer.name.ToUpper()}\nY: {playerPos.y + 1}\nX: {playerPos.x + 1}";
        p2Coordinates.text = $"{otherPlayer.name.ToUpper()}\nY: {otherPlayerPos.y + 1}\nX: {otherPlayerPos.x + 1}";

        grandmaHouseCoordinates.text = $"Casa de la Abuela\nY: {grandmaHousePos.y + 1}\nX: {grandmaHousePos.x + 1}";
        wolfDenCoordinates.text = $"Guarida del lobo\nY: {wolfDenPos.y + 1}\nX: {wolfDenPos.x + 1}";
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

    private void OnDisable()
    {
        if (zoneTooltipUI == null)
            return;

        zoneTooltipUI.HideTooltip();
    }
}