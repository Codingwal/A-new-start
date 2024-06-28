using System;
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
            River river = GenerateRiver(pos, rivers, seed, terrainSettings);
            if (river == null) continue;
            rivers.Add(river);

            if (rivers.Count == terrainSettings.maxRiverCount) break;
        }
        Debug.Log($"Generated {rivers.Count} rivers");

        SectorData sectorData = new() { rivers = rivers };
        return sectorData;
    }
    static River GenerateRiver(Vector2Int pos, List<River> rivers, int seed, TerrainSettings terrainSettings)
    {
        River river = new();

        {
            // Check if a river can be generated
            float height = VertexGenerator.GenerateVertexData(pos, seed, terrainSettings, terrainSettings.terrainScale);
            if (!CanBeRiverSource(height)) return null;

            // Debug.Log($"{pos} -> {height}");

            // Add the source vertex to the array
            river.points.Add(new(pos, height, 5));
        }


        Vector2 preferredDirection = FindClosestWater(pos, seed, terrainSettings).normalized;
        if (preferredDirection == new Vector2Int(0, 0))
        {
            return river;
        }
        // Debug.Log($"Direction = {preferredDirection} from pos {pos}");

        int i = 0;
        while (true)
        {
            // Get the next vertex and its height
            Vector2Int lowestNeighbourOffset = GetLowestNeighbourOffset(pos, rivers, preferredDirection, MapGenerator.Instance.riverFactor, seed, terrainSettings, out Vector2Int index);
            if (lowestNeighbourOffset == new Vector2Int(0, 0))
            {
                // Debug.Log("Stuck");
                break;
            }

            // If the river flowed into another river
            if (index != new Vector2Int(-1, -1))
            {
                River receivingRiver = rivers[index.x];

                receivingRiver.points[index.y].waterAmount += river.points[^1].waterAmount;
                // Debug.Log($"Rivers united! {receivingRiver.points[index.y - 1].waterAmount} + {river.points[^1].waterAmount} == {receivingRiver.points[index.y].waterAmount}");
                for (int j = index.y + 1; j < receivingRiver.points.Count; j++)
                {
                    receivingRiver.points[j].waterAmount = receivingRiver.points[j - 1].waterAmount + terrainSettings.riverWaterGain;
                }
                return river;
            }

            float lowestNeighbourHeight = VertexGenerator.GenerateVertexData(pos + lowestNeighbourOffset,
            seed, terrainSettings, terrainSettings.terrainScale);

            float height = Mathf.Min(lowestNeighbourHeight, river.points[^1].height - 0.0001f);

            pos += lowestNeighbourOffset;

            // Add the point to the river points list
            // If it's in the same chunk as the previous element, add it to that chunk, otherwise create a new entry and add it there
            river.points.Add(new(pos, height, river.points[^1].waterAmount + terrainSettings.riverWaterGain));

            // Break if the ocean or an existing river has been reached
            if (IsWater(height))
            {
                // Debug.Log("Landed in the ocean");
                break;
            }
            i++;

            if (i > 5000)
            {
                // Debug.Log("Terminated");
                break;
            }
        }
        return river;
    }
    static bool IsInRiver(List<River> rivers, Vector2Int point, out Vector2Int index)
    {
        foreach (River river in rivers)
        {
            for (int i = 0; i < river.points.Count; i += 10)
            {
                RiverPoint riverPoint = river.points[i];

                if (Mathf.Abs(riverPoint.pos.x - point.x) <= 10 && Mathf.Abs(riverPoint.pos.y - point.y) <= 10)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (i + j >= river.points.Count) break;

                        if (river.points[i + j].pos == point)
                        {
                            index = new(rivers.IndexOf(river), i + j);
                            return true;
                        }
                    }
                }
            }
        }
        index = new(-1, -1);
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
                return new();
            }
        }
    }
    static Vector2Int GetLowestNeighbourOffset(Vector2Int pos, List<River> rivers, Vector2 direction, float directionFactor, int seed, TerrainSettings terrainSettings, out Vector2Int index)
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

                if (IsInRiver(rivers, pos + new Vector2Int(x, y), out Vector2Int tempIndex))
                {
                    index = tempIndex;
                    return new(x, y);
                }

                lowestNeighbourHeight = neighbourHeight;
                lowestNeighbourOffset = new Vector2Int(x, y);
            }
        }

        index = new(-1, -1);
        return lowestNeighbourOffset;
    }
}
