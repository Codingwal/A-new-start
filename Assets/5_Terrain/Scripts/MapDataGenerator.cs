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
                BiomeSettings biomeSettings = BiomeSettings.Lerp(biomeSettings0, biomeSettingsY, (float)y / (chunkSize - 1) / terrainScale);
                map[x / vertexIncrement, y / vertexIncrement].height = VertexGenerator.GenerateVertexData(new Vector2(x + center.x, y + center.y), seed, biomeSettings, terrainScale);
            }
        }

        foreach (River river in sectorData.rivers)
        {
            foreach (Vector2Int point in river.points)
            {
                Vector2Int pointInChunkSpace = point - center + new Vector2Int(chunkSize / 2, chunkSize / 2);

                // If the point isn't in this chunk, continue
                if (pointInChunkSpace.x > chunkSize || pointInChunkSpace.x < 0 || pointInChunkSpace.y > chunkSize || pointInChunkSpace.y < 0) continue;

                AddIndent(map, 2, pointInChunkSpace);
            }
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
    static void AddIndent(VertexData[,] map, float strength, Vector2Int pos)
    {
        for (int y = -5; y <= 5; y++)
        {
            for (int x = -5; x <= 5; x++)
            {
                float distance = Mathf.Clamp(Mathf.Sqrt(x * x + y * y), 1, 100);
                int px = x + pos.x;
                int py = y + pos.y;

                if (px < 0 || px >= map.GetLength(0) || py < 0 || py >= map.GetLength(1)) continue;

                map[px, py].height -= Mathf.Clamp(strength - 0.5f * distance, 0, 50);

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
// static void GenerateRivers(VertexData[,] map, List<VertexToCalcInfo> transferredWater, Vector2Int center, int seed, float minWaterSourceHeight, float waterSlopeSpeedImpact)
//     {
//         // Temp (garantuees a water source in every chunk)
//         minWaterSourceHeight = -1;
//         float slopeTolerance = 1;

//         // parameters
//         int mapWidth = map.GetLength(0);
//         int mapHeight = map.GetLength(1);

//         Queue<VertexToCalcInfo> verticesToCalc = new();

//         // Try to generate a water source at a random position
//         {
//             System.Random rnd = new(seed);
//             // Vector2Int pos = new(rnd.Next(1, mapWidth - 1), rnd.Next(1, mapHeight - 1));
//             Vector2Int pos = new(mapWidth / 2, mapHeight / 2);
//             float value = map[pos.x, pos.y].height;

//             // Only generate if all requirements are met
//             if (value > minWaterSourceHeight)
//             {
//                 // 5 * 5 area with the pos in the center
//                 for (int x = -2; x <= 2; x++)
//                 {
//                     for (int y = -2; y <= 2; y++)
//                     {
//                         // 100% chance
//                         if (rnd.Next(1) == 0)
//                         {
//                             // generate source
//                             verticesToCalc.Enqueue(new(new(pos.x + x, pos.y + y), new(map[pos.x, pos.y].height, 1, new()), 1));
//                         }
//                     }
//                 }
//             }
//         }

//         foreach (VertexToCalcInfo vertexToCalcInfo in transferredWater)
//         {

//             verticesToCalc.Enqueue(vertexToCalcInfo);
//         }

//         int i = 0;
//         while (verticesToCalc.Count > 0)
//         {
//             // Get next vertex
//             VertexToCalcInfo info = verticesToCalc.Dequeue();
//             Vector2Int pos = info.pos;

//             if (map[pos.x, pos.y].waterAmount != 0) continue;
//             if (map[pos.x, pos.y].height == 0) continue;

//             i++;
//             if (i > 100000)
//             {
//                 Debug.LogError("Limit reached!");
//                 break;
//             }

//             VertexData data = new();

//             // Get the velocity and the water from the neighbour that functions as the source
//             VertexData neighbour = info.source;

//             data.waterAmount += neighbour.waterAmount * info.sourceInfluence;

//             // Get the direction and the height of lowest adjacent vertex
//             float lowestNeighbourHeight = float.PositiveInfinity;
//             Vector2Int lowestNeighbourOffset = new();

//             if (VertexHeightAtNeighbour(map, pos, new Vector2Int(1, 0)) < lowestNeighbourHeight)
//             {
//                 lowestNeighbourHeight = VertexHeightAtNeighbour(map, pos, new Vector2Int(1, 0));
//                 lowestNeighbourOffset = new(1, 0);
//             }
//             if (VertexHeightAtNeighbour(map, pos, new Vector2Int(-1, 0)) < lowestNeighbourHeight)
//             {
//                 lowestNeighbourHeight = VertexHeightAtNeighbour(map, pos, new Vector2Int(-1, 0));
//                 lowestNeighbourOffset = new(-1, 0);
//             }
//             if (VertexHeightAtNeighbour(map, pos, new Vector2Int(0, 1)) < lowestNeighbourHeight)
//             {
//                 lowestNeighbourHeight = VertexHeightAtNeighbour(map, pos, new Vector2Int(0, 1));
//                 lowestNeighbourOffset = new(0, 1);
//             }
//             if (VertexHeightAtNeighbour(map, pos, new Vector2Int(0, -1)) < lowestNeighbourHeight)
//             {
//                 lowestNeighbourHeight = VertexHeightAtNeighbour(map, pos, new Vector2Int(0, -1));
//                 lowestNeighbourOffset = new(0, -1);
//             }

//             // Calculate the slope from this vertex to the lowest adjacent vertex
//             float slope = VertexHeightAtNeighbour(map, pos, new(0, 0)) - lowestNeighbourHeight;

//             // If the lowest adjacent vertex is higher, stop the generation
//             if (slope < -slopeTolerance)
//             {
//                 map[pos.x, pos.y] = new(map[pos.x, pos.y].height, data.waterAmount, new());
//                 Debug.LogWarning("Too flat");
//                 continue;
//             }

//             // add the acceleration because of the slope, set magnitude to the calculated speed (the speed also depends on the slope)
//             float speed = slope * waterSlopeSpeedImpact;

//             // store the data in the array
//             map[pos.x, pos.y] = map[pos.x, pos.y] + data;

//             // Enqueue all effected vertices
//             Vector2Int affectedVertexPos = new(pos.x + lowestNeighbourOffset.x, pos.y);
//             if (affectedVertexPos.x >= 0)
//             {
//                 if (affectedVertexPos.x < mapWidth)
//                 {
//                     if (map[affectedVertexPos.x, affectedVertexPos.y].waterAmount == 0)
//                         verticesToCalc.Enqueue(new(affectedVertexPos, data, 1));
//                 }
//                 else
//                 {
//                     if (!transferredWaterDict.ContainsKey(center + Vector2Int.right))
//                         transferredWaterDict[center + Vector2Int.right] = new();
//                     transferredWaterDict[center + Vector2Int.right].Add(new(new(0, affectedVertexPos.y), data, 1));
//                     Debug.Log($"Sent water R {center * 240}");
//                 }
//             }
//             else
//             {
//                 if (!transferredWaterDict.ContainsKey(center + Vector2Int.left))
//                     transferredWaterDict[center + Vector2Int.left] = new();
//                 transferredWaterDict[center + Vector2Int.left].Add(new(new(map.GetLength(0) - 1, affectedVertexPos.y), data, 1));
//                 Debug.Log($"Sent water L {center * 240}");
//             }

//             affectedVertexPos = new(pos.x, pos.y + lowestNeighbourOffset.y);
//             if (affectedVertexPos.y >= 0)
//             {
//                 if (affectedVertexPos.y < mapHeight)
//                 {
//                     if (map[affectedVertexPos.x, affectedVertexPos.y].waterAmount == 0)
//                         verticesToCalc.Enqueue(new(affectedVertexPos, data, 1));
//                 }
//                 else
//                 {
//                     if (!transferredWaterDict.ContainsKey(center + Vector2Int.up))
//                         transferredWaterDict[center + Vector2Int.up] = new();
//                     transferredWaterDict[center + Vector2Int.up].Add(new(new(affectedVertexPos.x, 0), data, 1));
//                     Debug.Log($"Sent water U {center * 240}");
//                 }
//             }
//             else
//             {
//                 if (!transferredWaterDict.ContainsKey(center + Vector2Int.down))
//                     transferredWaterDict[center + Vector2Int.down] = new();
//                 transferredWaterDict[center + Vector2Int.down].Add(new(new(affectedVertexPos.x, map.GetLength(0) - 1), data, 1));
//                 Debug.Log($"Sent water D {center * 240}");
//             }


//         }

//         i = 0;
//         for (int y = 0; y < mapHeight; y++)
//         {
//             for (int x = 0; x < mapWidth; x++)
//             {
//                 map[x, y].height -= map[x, y].waterAmount * 0.7f;
//                 if (map[x, y].waterAmount > 0)
//                 {
//                     i++;
//                 }
//             }
//         }
//         // Debug.Log($"Water vertices count: {i}");
//     }
