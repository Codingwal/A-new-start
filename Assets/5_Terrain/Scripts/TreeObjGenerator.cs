using UnityEngine;

public static class TreeObjGenerator
{
    public static void InstantiateTrees(VertexData[,] map, Transform parent, GameObject prefab)
    {
        for (int x = 0; x < map.GetLength(0); x += 5)
        {
            for (int y = 0; y < map.GetLength(1); y += 5)
            {
                if (map[x, y].tree == 0) continue;

                GameObject newObj = GameObject.Instantiate(prefab, parent);

                newObj.transform.localPosition = new(x, map[x, y].height, y);
            }
        }
    }
}