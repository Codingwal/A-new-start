using System;
using UnityEngine;

public static class TreeObjGenerator
{
    public static void InstantiateTrees(MapData map, Transform parent, GameObject prefab)
    {
        foreach (TreeData tree in map.trees)
        {
            GameObject newObj = GameObject.Instantiate(prefab, parent);

            try
            {
                newObj.transform.localPosition = new(tree.pos.x, map.map[Mathf.RoundToInt(tree.pos.x), Mathf.RoundToInt(tree.pos.y)].height, tree.pos.y);
            }
            catch (Exception)
            {
                Debug.LogError(new Vector2(Mathf.RoundToInt(tree.pos.x), Mathf.RoundToInt(tree.pos.y)));
            }
        }
    }
}