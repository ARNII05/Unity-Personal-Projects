using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    private BorderZone[] borders;
    private Protector[] protectors;

    private void Awake()
    {
        borders = GetComponentsInChildren<BorderZone>(true);
        protectors = GetComponentsInChildren<Protector>(true);

        Transform portectorsParent = transform.Find("Protectors");

        if (!portectorsParent)
            return;

        portectorsParent.gameObject.SetActive(true);
    }

    public void Setup(Vector2Int mapPos, ZoneData[,] map)
    {
        Direction[] directions =
        {
            Direction.North,
            Direction.South,
            Direction.East,
            Direction.West
        };

        foreach (Direction direction in directions)
        {
            Vector2Int neighborPos = mapPos + GetDirectionOffset(direction);

            bool isCursed = map[mapPos.y, mapPos.x]
                .blockedDirections.Contains(direction);

            bool canMove = IsValid(neighborPos, map) && !isCursed;

            SetBorderAndProtector(direction, canMove);
            SetProtectorColor(direction, isCursed);
        }
    }

    private Vector2Int GetDirectionOffset(Direction direction)
    {
        return direction switch
        {
            Direction.North => Vector2Int.up,
            Direction.South => Vector2Int.down,
            Direction.East => Vector2Int.right,
            Direction.West => Vector2Int.left,
            _ => Vector2Int.zero
        };
    }

    private void SetProtectorColor(Direction direction, bool isCursed)
    {
        foreach (var protector in protectors)
        {
            if (protector.directionType != direction)
                continue;

            SpriteRenderer spriteRenderer =
                protector.GetComponentInChildren<SpriteRenderer>(true);

            if (spriteRenderer == null)
                continue;

            spriteRenderer.color = isCursed
                ? new Color32(152, 101, 201, 255)
                : Color.white;
        }
    }

    private void SetBorderAndProtector(Direction direction, bool active)
    {
        foreach (var border in borders)
        {
            if (border.direction == direction)
            {
                border.gameObject.SetActive(active);
            }
        }

        foreach (var protector in protectors)
        {
            if (protector.directionType == direction)
            {
                protector.gameObject.SetActive(!active);
            }
        }
    }

    private bool IsValid(Vector2Int pos, ZoneData[,] map)
    {
        return pos.x >= 0 && pos.x < map.GetLength(1) &&
               pos.y >= 0 && pos.y < map.GetLength(0);
    }
}
