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
    }

    public void Setup(Vector2Int mapPos, ZoneData[,] map)
    {
        foreach (var border in borders)
        {
            Direction direction = border.direction;
            Vector2Int offset = GetDirectionOffset(direction);
            Vector2Int neighborPos = mapPos + offset;
            bool isNeighborValid = IsValid(neighborPos, map);
            SetBorderAndProtector(direction, isNeighborValid);
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
