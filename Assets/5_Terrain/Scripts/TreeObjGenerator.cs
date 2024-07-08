using System;
using UnityEngine;

public static class TreeObjGenerator
{
    public static void InstantiateTrees(ChunkData map, Transform parent, SerializableDictonary<TreeTypes, GameObject> treePrefabs)
    {
        foreach (TreeData tree in map.trees)
        {
            GameObject prefab = treePrefabs[tree.type];

            GameObject newObj = GameObject.Instantiate(prefab, parent);

            newObj.transform.localPosition = new(tree.pos.x, map.map[Mathf.RoundToInt(tree.pos.x), Mathf.RoundToInt(tree.pos.y)].height - 0.3f, tree.pos.y);

            System.Random rnd = new((int)tree.pos.x * (int)tree.pos.y ^ 2);
            newObj.transform.rotation = Quaternion.Euler(0, rnd.Next(-180, 180), 0);
        }
    }
}