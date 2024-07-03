using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public static class TreeDataGenerator
{
    public static void GenerateTrees(VertexData[,] map, TerrainSettings terrainSettings, int seed, Vector2Int chunkCenter, int chunkSize)
    {
        const float perlinNoiseScale = 100;
        const float propabilityLerpMaxHeightBegin = 70;
        const float propabilityLerpMaxHeightEnd = 100;
        const float propabilityLerpMinHeightBegin = 15;
        const float propabilityLerpMinHeightEnd = 30;
        const float propabilityLerpMinSlope = 0.4f;
        const float propabilityLerpMaxSlope = 0.8f;

        float[,] propabilityMap = new float[map.GetLength(0), map.GetLength(1)];
        for (int x = 0; x < map.GetLength(0); x += 5)
        {
            for (int y = 0; y < map.GetLength(1); y += 5)
            {
                Vector2 pos = new Vector2(x, y) + chunkCenter - new Vector2(chunkSize / 2, chunkSize / 2);
                float probability = Mathf.PerlinNoise((seed + pos.x) / perlinNoiseScale, (seed + pos.y) / perlinNoiseScale);
                Debug.Log(probability);

                // Limit the probability to 1 so that exceeding the maxHeight or maxSlope garantuees that a tree isn't generated
                probability = Mathf.Min(probability, 1);
                // Debug.LogWarning(probability);

                // Reduce the propability for steep and/or high vertices
                probability -= Mathf.InverseLerp(propabilityLerpMaxHeightBegin, propabilityLerpMaxHeightEnd, map[x, y].height);
                probability -= Mathf.InverseLerp(propabilityLerpMinHeightEnd, propabilityLerpMinHeightBegin, map[x, y].height);
                probability -= Mathf.InverseLerp(propabilityLerpMinSlope, propabilityLerpMaxSlope, Slope(map, x, y));

                propabilityMap[x, y] = probability;
            }
        }

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                // Generate a deterministic, position and seed dependent random value between 0 and 1
                System.Random rnd = new(seed + 879 * x * y ^ 2);
                float value = (float)rnd.NextDouble();

                // The greater the propability, the likelier it is for a tree to be generated
                if (value < propabilityMap[x, y])
                    map[x, y].tree = 1;
            }
        }
    }
    static float Slope(VertexData[,] map, int x, int y)
    {
        // If one of these is zero, the following code will crash as index -1 doesnt exist
        x = Mathf.Max(1, x);
        y = Mathf.Max(1, y);

        return Mathf.Abs(map[x, y].height - map[x - 1, y - 1].height);
    }
}
