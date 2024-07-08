using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public static class TreeDataGenerator
{
    public static void GenerateTrees(ChunkData map, TerrainSettings terrainSettings, int seed, Vector2Int chunkCenter, int chunkSize)
    {
        int halfChunkSize = chunkSize / 2;

        const float perlinNoiseScale = 100;
        const float propabilityLerpMaxHeightBegin = 70;
        const float propabilityLerpMaxHeightEnd = 100;
        const float propabilityLerpMinHeightBegin = 15;
        const float propabilityLerpMinHeightEnd = 30;
        const float propabilityLerpMinSlope = 0.4f;
        const float propabilityLerpMaxSlope = 0.8f;
        const float maxPositioningOffset = 0.5f;
        // const float treeGenerationTolerance = 0f;

        // Generate a list of points (in chunkSpace) which can have a tree (spaced using BiomeSettings.minTreeSpacing)
        List<Vector2Int> pointsToGenerate = GetPointsToGenerate(map, chunkCenter, seed, chunkSize);

        foreach (Vector2Int point in pointsToGenerate)
        {
            // Convert the point to worldSpace
            Vector2Int pos = point + chunkCenter - new Vector2Int(chunkSize / 2, chunkSize / 2);

            // Generate a position and seed dependent random class (this ensures deterministic and completely seed dependent world generic)
            System.Random rnd = new(seed + 100 + pos.x * pos.y ^ 2);

            // Decide if a tree should be generated (depending on slope, height and perlin noise)
            {
                // Start with a probability between 0.5 and 1
                float probability = 0.5f + Mathf.PerlinNoise((seed + pos.x) / perlinNoiseScale, (seed + pos.y) / perlinNoiseScale) * 0.5f;

                // Reduce the probability at steep and high or low vertices
                probability -= Mathf.InverseLerp(propabilityLerpMaxHeightBegin, propabilityLerpMaxHeightEnd, map.map[point.x + halfChunkSize, point.y + halfChunkSize].height);
                probability -= Mathf.InverseLerp(propabilityLerpMinHeightEnd, propabilityLerpMinHeightBegin, map.map[point.x + halfChunkSize, point.y + halfChunkSize].height);
                probability -= Mathf.InverseLerp(propabilityLerpMinSlope, propabilityLerpMaxSlope, Slope(map.map, point.x + halfChunkSize, point.y + halfChunkSize));

                // The lower the probability, the more likely it is that this point will be skipped
                if (rnd.NextDouble() > probability) continue;
            }

            // Offset the position by a maximum of maxPositioningOffset to hide the underlying grid system
            Vector2 localPos = point;
            localPos += 2 * maxPositioningOffset * new Vector2((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);

            // Get all possible treeTypes and their corresponding chance
            List<BiomeTreeType> treeTypes = BiomeSettings.LerpTrees(BiomeSettings.LerpTrees(map.bottomLeft.trees, map.bottomRight.trees, point.x / (float)chunkSize),
                                                                   BiomeSettings.LerpTrees(map.topLeft.trees, map.topRight.trees, point.x / (float)chunkSize), point.y / (float)chunkSize);

            // Select a treeType and add the point to map.trees list
            // The resulting treeType can be null, which means that no tree is generated
            {
                float value = (float)rnd.NextDouble();
                TreeType type = null;

                // Foreach treeType (the same type can be present multiple times)...
                foreach (BiomeTreeType biomeTreeType in treeTypes)
                {
                    if (value > biomeTreeType.chance)
                    {
                        value -= biomeTreeType.chance;
                        continue;
                    }

                    TreeType treeType = biomeTreeType.treeType;

                    // TODO: Distance to next tree must be greater or equal to minDistance
                    type = treeType;
                    break;
                }
                // If a type has been set, add a tree with this type
                if (type != null)
                    map.trees.Add(new(localPos, type.tree));
            }
        }
    }
    static float Slope(VertexData[,] map, int x, int y)
    {
        // If one of these is zero, the following code will crash as index -1 doesn't exist
        // Because of this, they are set to a minimum of 1
        x = Mathf.Max(1, x);
        y = Mathf.Max(1, y);

        return Mathf.Abs(map[x, y].height - map[x - 1, y - 1].height);

    }
    static List<Vector2Int> GetPointsToGenerate(ChunkData chunk, Vector2Int chunkCenter, int seed, int chunkSize)
    {
        int halfChunkSize = chunkSize / 2;

        // Creates and fills a 2d array with chunk size with random values between 0 and 1
        float[,] randomValues = new float[chunkSize, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                randomValues[x, y] = GetRandomValue(seed, x + chunkCenter.x - halfChunkSize, y + chunkCenter.y - halfChunkSize);
            }
        }

        // Creates and fills a list with all points that should try to generate a tree
        List<Vector2Int> pointsToGenerate = new();
        for (int x = 0; x < chunkSize; x++)
        {
            float minTreeSpacingTop = Mathf.Lerp(chunk.topLeft.minTreeSpacing, chunk.topRight.minTreeSpacing, x / chunkSize);
            float minTreeSpacingBottom = Mathf.Lerp(chunk.bottomLeft.minTreeSpacing, chunk.bottomRight.minTreeSpacing, x / chunkSize);
            for (int y = 0; y < chunkSize; y++)
            {
                // Get the minTreeSpacing for this vertex
                float minTreeSpacing = Mathf.Lerp(minTreeSpacingBottom, minTreeSpacingTop, y / chunkSize);

                // If a tree should be generated here, add it to the pointsToGenerate list
                if (ShouldGenerateTree(randomValues, x, y, seed, Mathf.RoundToInt(minTreeSpacing)))
                    pointsToGenerate.Add(new(x - halfChunkSize, y - halfChunkSize));
            }
        }
        return pointsToGenerate;
    }

    /// <summary>Checks if all vertices closer than minTreeDistance have a lower value and returns true if this is the case</summary>
    static bool ShouldGenerateTree(float[,] randomValues, int x, int y, int seed, int minTreeDistance)
    {
        float value = randomValues[x, y];
        for (int offsetX = -minTreeDistance; offsetX <= minTreeDistance; offsetX++)
        {
            for (int offsetY = -minTreeDistance; offsetY <= minTreeDistance; offsetY++)
            {
                int px = x + offsetX;
                int py = y + offsetY;

                // If the point lies inside the chunk, simply get the neighbourValue out of the array
                // Else, calculate it using the GetRandomValue function
                float neighbourValue;
                if (px > 0 && py > 0 && px < randomValues.GetLength(0) && py < randomValues.GetLength(1))
                {
                    neighbourValue = randomValues[px, py];
                }
                else
                {
                    neighbourValue = GetRandomValue(seed, px, py);
                }

                // If the value of the neighbour is greater than the value of the point to be checked, no tree can be created
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
