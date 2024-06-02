using System.Collections.Generic;
using UnityEngine;

public static class RiverGenerator
{
    public static SectorData GenerateRivers(Vector2Int center, int sectorSize, int seed, TerrainSettings terrainSettings)
    {
        Vector2Int lowestPoint = new(center.x - sectorSize / 2, center.y - sectorSize / 2);
        Vector2Int highestPoint = new(center.x + sectorSize / 2, center.y + sectorSize / 2);

        List<River> rivers = new();

        System.Random rnd = new(seed);

        for (int i = 0; i < terrainSettings.maxRiverGenerationTries; i++)
        {
            Vector2Int pos = new(rnd.Next(lowestPoint.x, highestPoint.x), rnd.Next(lowestPoint.y, highestPoint.y));
            River river = GenerateRiver(pos, seed, terrainSettings);
            if (river == null) continue;
            rivers.Add(river);

            if (rivers.Count == terrainSettings.maxRiverCount) break;
        }
        Debug.Log($"Generated {rivers.Count} rivers");

        SectorData sectorData = new() { rivers = rivers };
        return sectorData;
    }
    static River GenerateRiver(Vector2Int pos, int seed, TerrainSettings terrainSettings)
    {
        float height = VertexGenerator.GenerateVertexData(pos, seed, terrainSettings, 1, terrainSettings.terrainScale);

        if (!CanBeRiverSource(height)) return null;
        Debug.Log($"{pos} -> {height}");

        River river = new();
        river.points.Add(pos);

        int i = 0;
        while (true)
        {
            // Get the direction and the height of the lowest adjacent vertex
            float lowestNeighbourHeight = float.PositiveInfinity;
            Vector2Int lowestNeighbourOffset = new();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    float neighbourHeight = VertexGenerator.GenerateVertexData(pos + new Vector2Int(x, y) * terrainSettings.riverStepSize, seed, terrainSettings, 1, terrainSettings.terrainScale);

                    if (neighbourHeight >= lowestNeighbourHeight) continue;

                    lowestNeighbourHeight = neighbourHeight;
                    lowestNeighbourOffset = new Vector2Int(x, y) * terrainSettings.riverStepSize;
                }
            }
            pos += lowestNeighbourOffset;

            river.points.Add(pos);

            // Break if the ocean or an existing river has been reached
            if (IsWater(lowestNeighbourHeight)) 
            {
                Debug.Log("Landed in ocean");
                break;
            }

            i++;
            if (i > 1000) 
            {
                Debug.Log("Terminated");
                break;
            }
        }
        return river;
    }
    static bool IsWater(float height)
    {
        return height < 0;
    }
    static bool CanBeRiverSource(float height)
    {
        return height > 50;
    }
}
