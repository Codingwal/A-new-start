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

    public static WorldData NewWorld(TerrainSettings terrainSettings, PlayerSettings playerSettings)
    {

        return new()
        {
            terrainSettings = terrainSettings,
            playerSettings = playerSettings,
            terrainData = TerrainData.Default,
            playerData = PlayerData.Default
        };
    }
    public WorldData()
    {
        terrainSettings = new();
        playerSettings = new();
        terrainData = TerrainData.Default;
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

    public static TerrainData Default
    {
        get
        {
            System.Random random = new();
            return new()
            {
                seed = random.Next(-1000000, 1000000),
                chunks = new()
            };
        }
    }
}

[Serializable]
public struct VertexWaterInfo
{
    public float amount;
    public Vector2 velocity;
    public VertexWaterInfo(float amount, Vector2 velocity)
    {
        this.amount = amount;
        this.velocity = velocity;
    }
    public static VertexWaterInfo operator +(VertexWaterInfo a, VertexWaterInfo b)
    {
        return new(a.amount + b.amount, a.velocity + b.velocity);
    }
}
[Serializable]
public class ChunkData
{
    public List<ListWrapper<float>> heightMap;
    public List<ListWrapper<VertexWaterInfo>> riverMap;
    public ChunkData(List<ListWrapper<float>> heightMap, List<ListWrapper<VertexWaterInfo>> riverMap)
    {
        this.heightMap = heightMap;
        this.riverMap = riverMap;
    }
}