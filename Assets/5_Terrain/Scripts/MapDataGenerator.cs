using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public static class MapDataGenerator
{
    static ConcurrentDictionary<Vector2Int, List<VertexToCalcInfo>> transferredWaterDict = new();

    public static ChunkData GenerateMapData(Vector2Int center, int chunkSize, int seed, TerrainSettings terrainSettings, SectorData sectorData, int vertexIncrement)
    {
        // Generate map data
        float terrainScale = terrainSettings.terrainScale;

        BiomeSettings biomeSettings00 = VertexGenerator.GetBiomeSettings(new Vector2(center.x - chunkSize / 2, center.y - chunkSize / 2) / terrainScale, terrainSettings, seed);
        BiomeSettings biomeSettingsX0 = VertexGenerator.GetBiomeSettings(new Vector2(center.x + chunkSize / 2, center.y - chunkSize / 2) / terrainScale, terrainSettings, seed);
        BiomeSettings biomeSettings0Y = VertexGenerator.GetBiomeSettings(new Vector2(center.x - chunkSize / 2, center.y + chunkSize / 2) / terrainScale, terrainSettings, seed);
        BiomeSettings biomeSettingsXY = VertexGenerator.GetBiomeSettings(new Vector2(center.x + chunkSize / 2, center.y + chunkSize / 2) / terrainScale, terrainSettings, seed);

        VertexData[,] map = new VertexData[(chunkSize - 1) / vertexIncrement + 1, (chunkSize - 1) / vertexIncrement + 1];
        for (int x = 0; x < chunkSize; x += vertexIncrement)
        {
            BiomeSettings biomeSettings0 = BiomeSettings.LerpTerrain(biomeSettings00, biomeSettingsX0, (float)x / (chunkSize - 1) / terrainScale);
            BiomeSettings biomeSettingsY = BiomeSettings.LerpTerrain(biomeSettings0Y, biomeSettingsXY, (float)x / (chunkSize - 1) / terrainScale);
            for (int y = 0; y < chunkSize; y += vertexIncrement)
            {
                // For each point...

                // Estimate the biomeSettings by lerping between the bottom and top value for this column
                BiomeSettings biomeSettings = BiomeSettings.LerpTerrain(biomeSettings0, biomeSettingsY, (float)y / (chunkSize - 1) / terrainScale);

                // Calculate the height using the biomeSettings
                map[x / vertexIncrement, y / vertexIncrement].height = VertexGenerator.GenerateVertexData(new Vector2(x + center.x - chunkSize / 2, y + center.y - chunkSize / 2), seed, biomeSettings, terrainSettings, terrainScale);
            }
        }

        const int riverRange = 100;
        const float waterAmountFactor = 1.3f;

        // Adjust terrain around the rivers so that they always flow downwards
        Dictionary<Vector2Int, float> pointsToChange = new();
        foreach (River river in sectorData.rivers)
        {
            foreach (RiverPoint point in river.points)
            {
                Vector2Int pointInChunkSpace = point.pos - center + new Vector2Int(chunkSize / 2, chunkSize / 2);

                // If the point is too far away to have an effect on this chunk, continue
                if (pointInChunkSpace.x >= chunkSize + riverRange || pointInChunkSpace.x < -riverRange || pointInChunkSpace.y >= chunkSize + riverRange || pointInChunkSpace.y < -riverRange) continue;

                AddIndent(pointInChunkSpace, point.height, map, pointsToChange, riverRange);
            }
        }
        foreach (KeyValuePair<Vector2Int, float> point in pointsToChange)
        {
            map[point.Key.x, point.Key.y].height = point.Value;
        }

        // Add the actual rivers
        List<List<Vector3>> rivers = new();
        foreach (River river in sectorData.rivers)
        {
            List<Vector3> riverPoints = new();
            foreach (RiverPoint point in river.points)
            {
                // Calculate the strength of the indentation by using the water amount
                float strength = Mathf.Pow(point.waterAmount, 1f / 3f) * waterAmountFactor;

                Vector2Int pointInChunkSpace = point.pos - center + new Vector2Int(chunkSize / 2, chunkSize / 2);

                // If the point is too far away to have an effect on this chunk, continue
                if (pointInChunkSpace.x >= chunkSize + strength || pointInChunkSpace.x < -strength || pointInChunkSpace.y >= chunkSize + strength || pointInChunkSpace.y < -strength) continue;

                // If the point lies inside the chunk, add it to the list  that will be used to generate the river mesh
                if (pointInChunkSpace.x < map.GetLength(0) && pointInChunkSpace.x > 0 && pointInChunkSpace.y < map.GetLength(1) && pointInChunkSpace.y > 0)
                    riverPoints.Add(new(pointInChunkSpace.x, map[pointInChunkSpace.x, pointInChunkSpace.y].height, pointInChunkSpace.y));

                for (int y = -Mathf.RoundToInt(strength); y <= strength; y++)
                {
                    for (int x = -Mathf.RoundToInt(strength); x <= strength; x++)
                    {
                        // Check if the point is inside this chunk  
                        int px = x + pointInChunkSpace.x;
                        int py = y + pointInChunkSpace.y;
                        if (px < 0 || px >= map.GetLength(0) || py < 0 || py >= map.GetLength(1)) continue;

                        float distance = Mathf.Sqrt(x * x + y * y);

                        map[px, py].height -= Mathf.SmoothStep(strength / 2, 0, Mathf.Clamp01(distance / strength));
                    }
                }
                // if (pointInChunkSpace.x < 0 || pointInChunkSpace.x >= map.GetLength(0) || pointInChunkSpace.y < 0 || pointInChunkSpace.y >= map.GetLength(1)) continue;
                // map[pointInChunkSpace.x, pointInChunkSpace.y].height = point.height;
            }
            rivers.Add(riverPoints);
        }

        return new(map, rivers, biomeSettings00, biomeSettingsX0, biomeSettings0Y, biomeSettingsXY);
    }
    static void AddIndent(Vector2Int pos, float height, VertexData[,] map, Dictionary<Vector2Int, float> pointsToChange, int riverRange)
    {
        for (int y = -riverRange; y <= riverRange; y++)
        {
            for (int x = -riverRange; x <= riverRange; x++)
            {
                // Check if the point is inside this chunk
                int px = x + pos.x;
                int py = y + pos.y;
                if (px < 0 || px >= map.GetLength(0) || py < 0 || py >= map.GetLength(1)) continue;

                // Calculate the distance to the central point
                float distance = Mathf.Sqrt(x * x + y * y);

                // Lerp between the original height and the height of the water by using the distance to the point
                float newHeight = Mathf.Lerp(height, map[px, py].height, Mathf.Clamp01(distance / riverRange));

                // Add it to the pointsToChange array if there isn't already is a lower height for the same point
                if (pointsToChange.ContainsKey(new(px, py)))
                    pointsToChange[new(px, py)] = Mathf.Min(pointsToChange[new(px, py)], newHeight);
                else
                    pointsToChange[new(px, py)] = newHeight;
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