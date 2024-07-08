using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public static class TreeDataGenerator
{
    public static void GenerateTrees(ChunkData map, TerrainSettings terrainSettings, int seed, Vector2Int chunkCenter, int chunkSize)
    {
        const float perlinNoiseScale = 100;
        const float propabilityLerpMaxHeightBegin = 70;
        const float propabilityLerpMaxHeightEnd = 100;
        const float propabilityLerpMinHeightBegin = 15;
        const float propabilityLerpMinHeightEnd = 30;
        const float propabilityLerpMinSlope = 0.4f;
        const float propabilityLerpMaxSlope = 0.8f;
        const float maxPositioningOffset = 0.5f;
        // const float treeGenerationTolerance = 0f;

        List<Vector2Int> pointsToGenerate = GetPointsToGenerate(map, chunkCenter, seed, chunkSize);

        foreach (Vector2Int point in pointsToGenerate)
        {
            Vector2Int pos = point + chunkCenter - new Vector2Int(chunkSize / 2, chunkSize / 2);
            float probability = Mathf.PerlinNoise((seed + pos.x) / perlinNoiseScale, (seed + pos.y) / perlinNoiseScale);

            // Limit the probability to 1 so that exceeding the maxHeight or maxSlope garantuees that a tree isn't generated
            probability = Mathf.Min(probability, 1);

            // Reduce the propability for steep and/or high vertices
            probability -= Mathf.InverseLerp(propabilityLerpMaxHeightBegin, propabilityLerpMaxHeightEnd, map.map[point.x, point.y].height);
            probability -= Mathf.InverseLerp(propabilityLerpMinHeightEnd, propabilityLerpMinHeightBegin, map.map[point.x, point.y].height);
            probability -= Mathf.InverseLerp(propabilityLerpMinSlope, propabilityLerpMaxSlope, Slope(map.map, point.x, point.y));

            System.Random rnd = new(seed + 100 + pos.x * pos.y ^ 2);

            if (rnd.NextDouble() > probability) continue;

            Vector2 localPos = point;
            localPos += 2 * maxPositioningOffset * new Vector2((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);

            // Select a treeType
            List<BiomeTreeType> treeTypes = BiomeSettings.LerpTrees(BiomeSettings.LerpTrees(map.bottomLeft.trees, map.bottomRight.trees, point.x / chunkSize),
                                                                   BiomeSettings.LerpTrees(map.topLeft.trees, map.topRight.trees, point.x / chunkSize), point.y / chunkSize);

            float val = (float)rnd.NextDouble();
            TreeType type = null;
            // Debug.Log("!");
            foreach (BiomeTreeType biomeTreeType in treeTypes)
            {
                // Debug.Log("!!!");
                if (val > biomeTreeType.chance)
                {
                    val -= biomeTreeType.chance;
                    continue;
                }

                TreeType treeType = biomeTreeType.treeType;

                // TODO: Distance to next tree must be greater or equal to minDistance
                type = treeType;
            }
            if (type != null)
                map.trees.Add(new(localPos, type.tree));
        }
    }
    static float Slope(VertexData[,] map, int x, int y)
    {
        // If one of these is zero, the following code will crash as index -1 doesnt exist
        x = Mathf.Max(1, x);
        y = Mathf.Max(1, y);

        return Mathf.Abs(map[x, y].height - map[x - 1, y - 1].height);

    }
    static List<Vector2Int> GetPointsToGenerate(ChunkData chunk, Vector2Int chunkCenter, int seed, int chunkSize)
    {
        List<Vector2Int> pointsToGenerate = new();

        float[,] randomValues = new float[chunkSize, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                randomValues[x, y] = GetRandomValue(seed, x + chunkCenter.x, y + chunkCenter.y);
            }
        }

        for (int x = 0; x < chunkSize; x++)
        {
            float minTreeSpacingTop = Mathf.Lerp(chunk.topLeft.minTreeSpacing, chunk.topRight.minTreeSpacing, x / chunkSize);
            float minTreeSpacingBottom = Mathf.Lerp(chunk.bottomLeft.minTreeSpacing, chunk.bottomRight.minTreeSpacing, x / chunkSize);
            for (int y = 0; y < chunkSize; y++)
            {
                float minTreeSpacing = Mathf.Lerp(minTreeSpacingBottom, minTreeSpacingTop, y / chunkSize);
                if (ShouldGenerateTree(randomValues, x, y, seed, Mathf.RoundToInt(minTreeSpacing)))
                    pointsToGenerate.Add(new(x, y));
            }
        }
        return pointsToGenerate;
    }
    static bool ShouldGenerateTree(float[,] randomValues, int x, int y, int seed, int minTreeDistance)
    {
        float value = GetRandomValue(seed, x, y);
        for (int offsetX = -minTreeDistance; offsetX <= minTreeDistance; offsetX++)
        {
            for (int offsetY = -minTreeDistance; offsetY <= minTreeDistance; offsetY++)
            {
                int px = x + offsetX;
                int py = y + offsetY;

                float neighbourValue;
                if (px > 0 && py > 0 && px < randomValues.GetLength(0) && py < randomValues.GetLength(1))
                {
                    neighbourValue = randomValues[px, py];
                }
                else
                {
                    neighbourValue = GetRandomValue(seed, px, py);
                }
                if (neighbourValue > value)
                    return false;
            }
        }
        return true;
    }
    static float GetRandomValue(int seed, int x, int y)
    {
        // Generate a deterministic, position and seed dependent random value between 0 and 1
        System.Random rnd = new(seed + x * y ^ 2);
        return (float)rnd.NextDouble();
    }
}
