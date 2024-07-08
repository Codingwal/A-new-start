using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/TreeType")]
[Serializable]
public class TreeTypeObject : ScriptableObject
{
    public TreeTypes tree;
    public float minDistance;
    public static explicit operator TreeType(TreeTypeObject obj)
    {
        return new()
        {
            tree = obj.tree,
            minDistance = obj.minDistance
        };
    }
}