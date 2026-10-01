using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderZone : MonoBehaviour
{
    public Direction direction;
    public Side side;
}

public enum Direction
{
    North,
    South,
    East,
    West,
    None,
}

public enum Side
{
    Right,
    Left,
}
