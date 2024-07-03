using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public static class TreeDataGenerator
{
    public static void GenerateTrees(VertexData[,] map, TerrainSettings terrainSettings, int seed)
    {
        Debug.LogWarning("1");
        const float perlinNoiseScale = 1;
        const float propabilityLerpMinHeight = 70;
        const float propabilityLerpMaxHeight = 100;
        const float propabilityLerpMinSlope = 0.4f;
        const float propabilityLerpMaxSlope = 0.8f;

        float[,] propabilityMap = new float[map.GetLength(0), map.GetLength(1)];
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                float probability = Mathf.PerlinNoise((x + 100) * perlinNoiseScale, (y + 100) * perlinNoiseScale);

                // Limit the probability to 1 so that exceeding the maxHeight or maxSlope garantuees that a tree isn't generated
                probability = Mathf.Min(probability, 1);

                // Reduce the propability for steep and/or high vertices
                // probability -= Mathf.InverseLerp(propabilityLerpMinHeight, propabilityLerpMaxHeight, map[x, y].height);
                // probability -= Mathf.InverseLerp(propabilityLerpMinSlope, propabilityLerpMaxSlope, Slope(map, x, y));

                propabilityMap[x, y] = probability;
            }
        }
        Debug.LogWarning("2");

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                // // Generate a deterministic, position and seed dependent random value between 0 and 0.5 and add it to the propability
                // System.Random rnd = new(seed + 879 * x * y ^ 2);
                // probability += (float)rnd.NextDouble() / 2;

                if (propabilityMap[x, y] > 0.1f)
                    map[x, y].tree = 1;
            }
        }
        Debug.LogWarning("3");
    }
    static float Slope(VertexData[,] map, int x, int y)
    {
        // If one of these is zero, the following code will crash as index -1 doesnt exist
        x = Mathf.Max(1, x);
        y = Mathf.Max(1, y);

        return Mathf.Abs(map[x, y].height - map[x - 1, y - 1].height);
    }
}
