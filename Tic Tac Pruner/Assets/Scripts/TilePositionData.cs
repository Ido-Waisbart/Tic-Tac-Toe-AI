using System;
using UnityEngine;

public class TilePositionData : MonoBehaviour
{
    [NonSerialized] public int x;
    [NonSerialized] public int y;
    public int IndexInGrid => x + y * 3;
}
