using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class RiverGenerator
{
    public static SectorData GenerateRivers(Vector2Int center, int seed, int sectorSize, TerrainSettings terrainSettings)
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
        River river = new();

        {
            // Check if a river can be generated
            float height = VertexGenerator.GenerateVertexData(pos, seed, terrainSettings, terrainSettings.terrainScale);
            if (!CanBeRiverSource(height)) return null;

            Debug.Log($"{pos} -> {height}");

            // Add the source vertex to the array
            river.points.Add(new(pos, height, 1));
        }


        Vector2 preferredDirection = FindClosestWater(pos, seed, terrainSettings).normalized;
        if (preferredDirection == new Vector2Int(0, 0))
        {
            return river;
        }
        Debug.Log($"Direction = {preferredDirection} from pos {pos}");

        int i = 0;
        while (true)
        {
            // Get the next vertex and its height
            Vector2Int lowestNeighbourOffset = GetLowestNeighbourOffset(pos, river, preferredDirection, MapGenerator.Instance.riverFactor, seed, terrainSettings);
            if (lowestNeighbourOffset == new Vector2Int(0, 0))
            {
                Debug.LogError("Stuck");
                break;
            }
            float lowestNeighbourHeight = VertexGenerator.GenerateVertexData(pos + lowestNeighbourOffset,
            seed, terrainSettings, terrainSettings.terrainScale);

            float height = Mathf.Min(lowestNeighbourHeight, river.points[^1].height - 0.0001f);

            pos += lowestNeighbourOffset;

            // Add the point to the river points list
            // If it's in the same chunk as the previous element, add it to that chunk, otherwise create a new entry and add it there
            river.points.Add(new(pos, height, river.points[^1].waterAmount + 0.001f));

            // Break if the ocean or an existing river has been reached
            if (IsWater(height))
            {
                Debug.Log("Landed in the ocean");
                break;
            }
            i++;

            if (i > 5000)
            {
                Debug.LogError("Terminated");
                break;
            }
        }
        return river;
    }
    static Vector2Int GetChunk(Vector2Int point, int chunkSize)
    {
        return Vector2Int.RoundToInt(point / chunkSize) * chunkSize;
    }
    static bool IsInRiver(River river, Vector2Int point)
    {
        // return river.points.Contains(point);
        return false;
    }
    static bool IsWater(float height)
    {
        return height < 0;
    }
    static bool CanBeRiverSource(float height)
    {
        return height > 50;
    }
    static Vector2 FindClosestWater(Vector2Int pos, int seed, TerrainSettings terrainSettings)
    {
        int distance = 10;
        while (true)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    Vector2Int point = pos + new Vector2Int(x, y) * distance;
                    float height = VertexGenerator.GenerateVertexData(point, seed, terrainSettings, terrainSettings.terrainScale);

                    if (IsWater(height))
                    {
                        return point;
                    }
                }
            }
            distance += 50;

            if (distance > 1000)
            {
                Debug.LogError("No water found");
                return new();
            }
        }
    }
    static Vector2Int GetLowestNeighbourOffset(Vector2Int pos, River river, Vector2 direction, float directionFactor, int seed, TerrainSettings terrainSettings)
    {
        // Get the direction and the height of the lowest adjacent vertex
        float lowestNeighbourHeight = float.PositiveInfinity;
        Vector2Int lowestNeighbourOffset = new();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                // How wrong would this direction be? (Between 0 and 4)
                float directionError = Mathf.Abs(x - direction.x) + Mathf.Abs(y - direction.y);

                if (directionError > 2) continue;

                float neighbourHeight = VertexGenerator.GenerateVertexData(pos + new Vector2Int(x, y),
                seed, terrainSettings, terrainSettings.terrainScale, 2);

                neighbourHeight += directionError * directionFactor;
                // Debug.Log($"({x}, {y}) -> {(Mathf.Abs(x - direction.x) + Mathf.Abs(y - direction.y)) * directionFactor}");

                if (neighbourHeight >= lowestNeighbourHeight) continue;

                if (IsInRiver(river, pos + new Vector2Int(x, y)))
                    continue;

                lowestNeighbourHeight = neighbourHeight;
                lowestNeighbourOffset = new Vector2Int(x, y);
            }
        }

        return lowestNeighbourOffset;
    }
}
