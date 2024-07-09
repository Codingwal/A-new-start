using System;
using System.Collections.Generic;
using UnityEngine;

public static class TreeObjGenerator
{
    public static List<GameObject> InstantiateTrees(ChunkData map, Transform parent, SerializableDictonary<TreeTypes, GameObject> treePrefabs)
    {
        int halfChunkSize = MapGenerator.Instance.chunkSize / 2;

        List<GameObject> trees = new(map.trees.Count);
        foreach (TreeData tree in map.trees)
        {
            GameObject prefab = treePrefabs[tree.type];

            GameObject newObj = GameObject.Instantiate(prefab, parent);

            newObj.transform.localPosition = new(tree.pos.x, map.map[Mathf.RoundToInt(tree.pos.x + halfChunkSize), Mathf.RoundToInt(tree.pos.y + halfChunkSize)].height - 0.3f, tree.pos.y);

            System.Random rnd = new((int)tree.pos.x * (int)tree.pos.y ^ 2);

            // Gives the object a random rotation
            newObj.transform.rotation = Quaternion.Euler(0, rnd.Next(-180, 180), 0);

            trees.Add(newObj);
        }
        return trees;
    }
}