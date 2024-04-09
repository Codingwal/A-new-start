using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class MapDataHandler : Singleton<MapDataHandler>, IDataCallbackReceiver
{
    // The chunk dictionary
    public ConcurrentDictionary<Vector2, ChunkData> chunks;
    public WorldData worldData;

    public void LoadData(WorldData worldData)
    {
        // Prevent endlessTerrain from trying to load deleted chunks
        EndlessTerrain.terrainChunkDictonary.Clear();
        EndlessTerrain.terrainChunksVisibleLastUpdate.Clear();
        
        MapGenerator.Instance.terrainSettings = DataManager.GetWorldSettings().terrainSettings;

        this.worldData = worldData;
        chunks = worldData.terrainData.chunks;

        Debug.Log($"Loaded {chunks.Count} chunks");

    }
    public void SaveData(WorldData worldData)
    {
        worldData.terrainData.chunks = new SerializableDictonary<Vector2, ChunkData>();
        // worldData.terrainData.chunks = (SerializableDictonary<Vector2, ChunkData>)chunks;

        Debug.Log($"Saved {worldData.terrainData.chunks.Count} chunks");
    }

    // EndlessTerrain uses this to add chunks
    public void AddChunk(Vector2 center, float[,] heightMap)
    {
        List<ListWrapper<float>> tempHeightMap = new();
        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            ListWrapper<float> temp = new();
            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                temp.list.Add(heightMap[x, y]);
            }
            tempHeightMap.Add(temp);
        }
        chunks[center] = new(tempHeightMap);
    }
}

