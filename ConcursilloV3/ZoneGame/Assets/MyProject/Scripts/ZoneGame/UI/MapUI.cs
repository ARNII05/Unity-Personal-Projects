using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class MapUI : MonoBehaviour
{
    public static MapUI Instance { get; private set; }

    [SerializeField] private GameObject mapObj;
    
    private GameObject mapZones;
    private TextMeshProUGUI zoneName;
    private TextMeshProUGUI p1Coordinates;
    private TextMeshProUGUI p2Coordinates;
    private TextMeshProUGUI grandmaHouseCoordinates;
    private TextMeshProUGUI wolfDenCoordinates;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        mapZones = mapObj.transform.Find("MapZones").gameObject;
        zoneName = mapObj.transform.Find("ZoneName").GetComponent<TextMeshProUGUI>();
        p1Coordinates = mapObj.transform.Find("P1Coordinates").GetComponent<TextMeshProUGUI>();
        p2Coordinates = mapObj.transform.Find("P2Coordinates").GetComponent<TextMeshProUGUI>();
        grandmaHouseCoordinates = mapObj.transform.Find("GrandmaHouseCoordinates").GetComponent<TextMeshProUGUI>();
        wolfDenCoordinates = mapObj.transform.Find("WolfDenCoordinates").GetComponent<TextMeshProUGUI>();
        mapObj.SetActive(false);
    }

    public void ToggleMap(Player player)
    {
        if (mapObj.activeSelf)
            OnClose(player);
        else
            OnOpen(player);
    }

    private void OnOpen(Player player)
    {
        player.State = PlayerState.UsingMap;
        mapObj.SetActive(true);
    }

    private void OnClose(Player player)
    {
        player.State = PlayerState.Normal;
        mapObj.SetActive(false);
    }

    public void UpdateMap(Player player)
    {
        ZoneData[,] map = MapManager.Instance.map;

        UpdateCoordinates(map, player);

        UpdateZoneIcons(map, player);
    }

    private void UpdateCoordinates(ZoneData[,] map, Player player)
    {
        Player otherPlayer = MapManager.Instance.GetOtherPlayer(player);

        Vector2Int playerPos = player.NetworkMapPos.Value;
        Vector2Int otherPlayerPos = otherPlayer.NetworkMapPos.Value;
        Vector2Int grandmaHousePos = MapManager.Instance.grandmaPos;
        Vector2Int wolfDenPos = MapManager.Instance.wolfDenPos;

        ZoneData currentZone = map[playerPos.y, playerPos.x];

        zoneName.text = currentZone.GetName();

        p1Coordinates.text = $"{player.name.ToUpper()}\nX: {playerPos.x + 1}\nY: {playerPos.y + 1}";
        p2Coordinates.text = $"{otherPlayer.name.ToUpper()}\nX: {otherPlayerPos.x + 1}\nY: {otherPlayerPos.y + 1}";

        grandmaHouseCoordinates.text = $"Casa de la Abuela\nX: {grandmaHousePos.x + 1}\nY: {grandmaHousePos.y + 1}";
        wolfDenCoordinates.text = $"Guarida del lobo\nX: {wolfDenPos.x + 1}\nY: {wolfDenPos.y + 1}";
    }

    private void UpdateZoneIcons(ZoneData[,] map, Player player)
    {
        GameObject actualZone = mapZones.transform.Find("ActualZone").gameObject;
        GameObject northZone = mapZones.transform.Find("NorthZone").gameObject;
        GameObject southZone = mapZones.transform.Find("SouthZone").gameObject;
        GameObject eastZone = mapZones.transform.Find("EastZone").gameObject;
        GameObject westZone = mapZones.transform.Find("WestZone").gameObject;

        Vector2Int playerPos = player.NetworkMapPos.Value;

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
}