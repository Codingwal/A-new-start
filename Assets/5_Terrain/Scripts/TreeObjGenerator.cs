using UnityEngine;

public static class TreeObjGenerator
{
    public static void InstantiateTrees(VertexData[,] map, Transform parent, GameObject prefab)
    {
        Debug.LogWarning("4");
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y].tree == 0) continue;

                GameObject newObj = GameObject.Instantiate(prefab, parent);

                newObj.transform.localPosition = new(x, map[x, y].height, y);
            }
        }
        Debug.LogWarning("5");
    }
}