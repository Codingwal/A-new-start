using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public static class MapDataGenerator
{
    static ConcurrentDictionary<Vector2Int, List<VertexToCalcInfo>> transferredWaterDict = new();

    public static MapData GenerateMapData(Vector2Int center, int chunkSize, int seed, TerrainSettings terrainSettings, SectorData sectorData, int vertexIncrement)
    {
        // Generate map data
        float terrainScale = terrainSettings.terrainScale;

        Vector2[] biomeOctaveOffsets = VertexGenerator.GenerateOctaveOffsets(seed, 5);
        BiomeSettings biomeSettings00 = VertexGenerator.GetBiomeSettings(new Vector2(center.x, center.y) / terrainScale, biomeOctaveOffsets, terrainSettings);
        BiomeSettings biomeSettingsX0 = VertexGenerator.GetBiomeSettings(new Vector2(center.x + chunkSize - 1, center.y) / terrainScale, biomeOctaveOffsets, terrainSettings);
        BiomeSettings biomeSettings0Y = VertexGenerator.GetBiomeSettings(new Vector2(center.x, center.y + chunkSize - 1) / terrainScale, biomeOctaveOffsets, terrainSettings);
        BiomeSettings biomeSettingsXY = VertexGenerator.GetBiomeSettings(new Vector2(center.x + chunkSize - 1, center.y + chunkSize - 1) / terrainScale, biomeOctaveOffsets, terrainSettings);

        VertexData[,] map = new VertexData[(chunkSize - 1) / vertexIncrement + 1, (chunkSize - 1) / vertexIncrement + 1];
        for (int x = 0; x < chunkSize; x += vertexIncrement)
        {
            BiomeSettings biomeSettings0 = BiomeSettings.Lerp(biomeSettings00, biomeSettingsX0, (float)x / (chunkSize - 1) / terrainScale);
            BiomeSettings biomeSettingsY = BiomeSettings.Lerp(biomeSettings0Y, biomeSettingsXY, (float)x / (chunkSize - 1) / terrainScale);
            for (int y = 0; y < chunkSize; y += vertexIncrement)
            {
                // For each point...


                // Estimate the biomeSettings by lerping between the bottom and top value for this column
                BiomeSettings biomeSettings = BiomeSettings.Lerp(biomeSettings0, biomeSettingsY, (float)y / (chunkSize - 1) / terrainScale);

                // Calculate the height using the biomeSettings
                map[x / vertexIncrement, y / vertexIncrement].height = VertexGenerator.GenerateVertexData(new Vector2(x + center.x, y + center.y), seed, biomeSettings, terrainScale);
            }
        }

        Dictionary<Vector2Int, float> pointsToChange = new();
        foreach (River river in sectorData.rivers)
        {
            foreach (RiverPoint point in river.points)
            {
                Vector2Int pointInChunkSpace = point.pos - center;

                // If the point isn't in this chunk, continue
                if (pointInChunkSpace.x >= chunkSize || pointInChunkSpace.x < 0 || pointInChunkSpace.y >= chunkSize || pointInChunkSpace.y < 0) continue;

                AddIndent(pointInChunkSpace, point.height, map, pointsToChange, 5);
            }
        }
        foreach (KeyValuePair<Vector2Int, float> point in pointsToChange)
        {
            map[point.Key.x, point.Key.y].height = point.Value;
        }

        return new(map);
    }
    static float VertexHeightAtNeighbour(VertexData[,] map, Vector2Int pos, Vector2Int offset)
    {
        if (pos.x + offset.x < map.GetLength(0) && pos.x + offset.x >= 0 && pos.y + offset.y < map.GetLength(1) && pos.y + offset.y >= 0)
        {
            VertexData vertexData = map[pos.x + offset.x, pos.y + offset.y];
            return vertexData.height + vertexData.waterAmount * 10;
        }
        else
        {
            // Estimate the position by continuing the slope of the vertex in the opposite direction
            return 2 * map[pos.x, pos.y].height - map[pos.x - offset.x, pos.y - offset.y].height;
        }

    }
    static void AddIndent(Vector2Int pos, float height, VertexData[,] map, Dictionary<Vector2Int, float> pointsToChange, float strength)
    {
        // TODO: Why is map[pos].height sometimes lower than height???
        float heightDifferenceAtPoint = Mathf.Max(0.5f, map[pos.x, pos.y].height - height);

        for (int y = -20; y <= 20; y++)
        {
            for (int x = -20; x <= 20; x++)
            {
                // Check if the point is inside this chunk
                int px = x + pos.x;
                int py = y + pos.y;
                if (px < 0 || px >= map.GetLength(0) || py < 0 || py >= map.GetLength(1)) continue;

                // Calculate the distance to the central point
                float distance = Mathf.Sqrt(x * x + y * y);

                // float reduceHeightBy = Mathf.Lerp(heightDifferenceAtPoint, 0, distance / 141.5f);
                float newHeight = Mathf.Lerp(height, map[px, py].height, Mathf.Clamp01(distance / 30));

                // Debug.Log($"{height}, {map[px, py].height}, {Mathf.Clamp01(distance / 141.5f)} -> {newHeight}");

                if (pointsToChange.ContainsKey(new(px, py)))
                    pointsToChange[new(px, py)] = Mathf.Min(pointsToChange[new(px, py)], newHeight);
                else
                    pointsToChange[new(px, py)] = newHeight;

                // map[px, py].height = Mathf.Max(height, map[px, py].height);

                if (x == 0 && y == 0)
                    // map[px, py].height = height - strength;
                    map[px, py].height = height - strength;
            }
        }
    }
    struct VertexToCalcInfo
    {
        public Vector2Int pos;
        public VertexData source;
        public float sourceInfluence;
        public VertexToCalcInfo(Vector2Int pos, VertexData source, float sourceInfluence)
        {
            this.pos = pos;
            this.source = source;
            this.sourceInfluence = sourceInfluence;
        }
    }
}