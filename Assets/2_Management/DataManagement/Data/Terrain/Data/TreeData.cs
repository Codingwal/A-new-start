using UnityEngine;

public struct TreeData
{
    public Vector2 pos;
    public TreeTypes type;
    public TreeData(Vector2 pos, TreeTypes type)
    {
        this.pos = pos;
        this.type = type;
    }
}
public enum TreeTypes
{
    None = 0,
    Maple,
    Oak
}
