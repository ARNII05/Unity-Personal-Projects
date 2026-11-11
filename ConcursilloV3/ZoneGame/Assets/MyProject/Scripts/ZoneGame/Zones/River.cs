using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class River : MonoBehaviour
{
    [SerializeField] private GameObject[] bridges;
    [SerializeField] private GameObject[] riverInteractors;
    [SerializeField] private GameObject[] triggers;
    [SerializeField] private RiverType riverType;
    
    public Vector2Int mapPosition;
    private ZoneData[,] map;

    [SerializeField] private float pulseDuration = 2f;
    [SerializeField] private float minAlpha = 0.1f;
    [SerializeField] private float maxAlpha = 0.6f;

    private SpriteRenderer[] bridgeRenderers;
    private Color[] originalColors;

    private bool isBridged;

    private void Awake()
    {
        bridgeRenderers = bridges
            .Where(bridge => bridge != null)
            .SelectMany(bridge => bridge.GetComponentsInChildren<SpriteRenderer>(true))
            .ToArray();

        originalColors = new Color[bridgeRenderers.Length];

        for (int i = 0; i < bridgeRenderers.Length; i++)
        {
            originalColors[i] = bridgeRenderers[i].color;
        }
    }

    public void Init(Vector2Int mapPosition)
    {
        this.mapPosition = mapPosition;
        map = MapManager.Instance.map;

        isBridged = map[mapPosition.y, mapPosition.x].riverBridged;

        SwapGameObjectStatus(true, bridges);
        SwapGameObjectStatus(!isBridged, riverInteractors);
        SwapGameObjectStatus(!isBridged, triggers);

        InitHorizontalRiver();
    }

    private void Update()
    {
        if (isBridged || bridgeRenderers == null)
            return;

        float alpha = Mathf.Lerp(
            minAlpha,
            maxAlpha,
            (Mathf.Sin(Time.time * Mathf.PI * 2f / pulseDuration) + 1f) / 2f
        );

        Direction direction = map[mapPosition.y, mapPosition.x].firstRiverDirection;

        for (int i = 0; i < bridgeRenderers.Length; i++)
        {
            SpriteRenderer sprite = bridgeRenderers[i];

            if (sprite == null || !sprite.gameObject.activeInHierarchy)
                continue;

            bool shouldAnimate = true;

            if (riverType == RiverType.Horizontal)
            {
                if (direction == Direction.North)
                {
                    shouldAnimate = sprite.transform.IsChildOf(bridges[1].transform);
                }
                else if (direction == Direction.South)
                {
                    shouldAnimate = sprite.transform.IsChildOf(bridges[0].transform);
                }
            }

            sprite.color = shouldAnimate
                ? new Color(1f, 1f, 1f, alpha)
                : originalColors[i];
        }
    }

    private void InitHorizontalRiver()
    {
        if (riverType != RiverType.Horizontal || isBridged)
            return;

        Direction direction = map[mapPosition.y, mapPosition.x].firstRiverDirection;

        riverInteractors[0].SetActive(direction != Direction.North);
        riverInteractors[1].SetActive(direction != Direction.South);
        riverInteractors[2].SetActive(direction == Direction.None);
    }

    public bool Interact(Player player)
    {
        if (isBridged)
            return false;

        if (player.inventory.GetAmount(ItemType.Log) < CraftingSystem.logsForBridge)
            return false;

        return true;
    }

    private void SwapGameObjectStatus(bool status, GameObject[] objects)
    {
        foreach (var item in objects)
        {
            item.SetActive(status);
        }
    }

    public void SwapGameObjectStatus()
    {
        isBridged = true;

        SwapGameObjectStatus(false, riverInteractors);
        SwapGameObjectStatus(false, triggers);
        SwapGameObjectStatus(true, bridges);

        for (int i = 0; i < bridgeRenderers.Length; i++)
        {
            bridgeRenderers[i].color = originalColors[i];
        }
    }
}

public enum RiverType
{
    Horizontal,
    Vertical
}