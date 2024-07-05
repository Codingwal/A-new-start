using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldData
{
    public PlayerSettings playerSettings;
    public PlayerData playerData;
    public TerrainSettings terrainSettings;
    public TerrainData terrainData;

    public static WorldData NewWorld(TerrainSettings terrainSettings, PlayerSettings playerSettings, int seed)
    {

        return new()
        {
            terrainSettings = terrainSettings,
            playerSettings = playerSettings,
            terrainData = TerrainData.Default(seed),
            playerData = PlayerData.Default
        };
    }
    public WorldData()
    {
        terrainSettings = new();
        playerSettings = new();
        terrainData = TerrainData.Default(0);
        playerData = PlayerData.Default;
    }
}
[Serializable]
public class PlayerData
{
    [SerializeField] public Vector3 position;
    [SerializeField] public Vector3 rotation;

    public PlayerData()
    {
        position = new(0, 200, 0);
        rotation = new();
    }

    public static PlayerData Default
    {
        get
        {
            return new()
            {
                position = new(0, 200, 0),
                rotation = new(),
            };
        }
    }
}
[Serializable]
public class TerrainData
{
    public int seed;
    [SerializeField] public SerializableDictonary<Vector2, ChunkData> chunks;

    public static TerrainData Default(int seed)
    {
        System.Random random = new();
        return new()
        {
            seed = seed,
            chunks = new()
        };
    }
}

[Serializable]
public struct VertexData
{
    public float height;
    public float waterAmount;
}
[Serializable]
public class ChunkData
{
    public BiomeSettings bottomLeft;
    public BiomeSettings bottomRight;
    public BiomeSettings topLeft;
    public BiomeSettings topRight;
    public List<ListWrapper<VertexData>> map;
    public List<ListWrapper<Vector3>> rivers;
    public List<TreeData> trees;
    public ChunkData() { }
    public ChunkData(List<ListWrapper<VertexData>> map, List<ListWrapper<Vector3>> rivers, List<TreeData> trees)
    {
        this.map = map;
        this.rivers = rivers;
        this.trees = trees;
    }
}
public class SectorData
{
    public List<River> rivers = new();
}
public class River
{
    public List<RiverPoint> points = new();
}
public class RiverPoint
{
    public Vector2Int pos;
    public float height;
    public float waterAmount;
    public RiverPoint(Vector2Int pos, float height, float waterAmount)
    {
        this.pos = pos;
        this.height = height;
        this.waterAmount = waterAmount;
    }
}
public struct TreeData
{
    public Vector2 pos;
    public TreeTypes type;
    public TreeData(Vector2 pos, TreeTypes type)
    {
        this.pos = pos;
        this.type = type;
    }
}