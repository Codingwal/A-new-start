using System.Collections.Generic;
using UnityEngine;

public class River
{
    public List<RiverPoint> points = new();
}
public class RiverPoint
{
    public Vector2Int pos;
    public float height;
    public float waterAmount;
    public RiverPoint(Vector2Int pos, float height, float waterAmount)
    {
        this.pos = pos;
        this.height = height;
        this.waterAmount = waterAmount;
    }
}