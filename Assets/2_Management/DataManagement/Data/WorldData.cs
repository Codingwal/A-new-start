using UnityEngine;
using System;
using System.Collections.Generic;

public class WorldData
{
    public string worldSettingsName;
    public PlayerData playerData;
    public TerrainData terrainData;
    

    public static WorldData NewWorld(string worldSettingsName)
    {
        
        return new()
        {
            worldSettingsName = worldSettingsName,
            terrainData = TerrainData.Default,
            playerData = PlayerData.Default
        };
    }
}
[Serializable]
public class PlayerData
{
    public Vector3 position;
    public Vector3 rotation;

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
    public SerializableDictonary<Vector2, ChunkData> chunks;

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
public class ChunkData
{
    public List<ListWrapper<float>> heightMap;
    public ChunkData(List<ListWrapper<float>> heightMap)
    {
        this.heightMap = heightMap;
    }
}

