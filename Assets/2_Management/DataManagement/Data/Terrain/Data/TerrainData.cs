using System.Collections.Generic;
using UnityEngine;

public class TerrainData
{
    public int seed;
    public Dictionary<Vector2, ChunkData> chunks;

    public static TerrainData Default(int seed)
    {
        return new()
        {
            seed = seed,
            chunks = new()
        };
    }
}
public class SectorData
{
    public List<River> rivers = new();
}
public class ChunkData
{
    public BiomeSettings bottomLeft;
    public BiomeSettings bottomRight;
    public BiomeSettings topLeft;
    public BiomeSettings topRight;
    public VertexData[,] map;
    public List<List<Vector3>> rivers;
    public List<TreeData> trees;
    public ChunkData() { }
    public ChunkData(VertexData[,] map, List<List<Vector3>> rivers, BiomeSettings bottomLeft, BiomeSettings bottomRight, BiomeSettings topLeft, BiomeSettings topRight)
    {
        this.map = map;
        this.rivers = rivers;
        trees = new();

        this.bottomLeft = bottomLeft;
        this.bottomRight = bottomRight;
        this.topLeft = topLeft;
        this.topRight = topRight;
    }
}
public struct VertexData
{
    public float height;
    public float waterAmount;
}