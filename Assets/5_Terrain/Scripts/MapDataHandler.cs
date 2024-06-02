using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class MapDataHandler : Singleton<MapDataHandler>, IDataCallbackReceiver
{
    // The chunk dictionary
    public ConcurrentDictionary<Vector2, ChunkData> chunks;
    public ConcurrentDictionary<Vector2, SectorData> sectors;
    public ConcurrentDictionary<Vector2, Vector2[]> chunksWaitingForSector;
    public WorldData worldData;

    public void LoadData(WorldData worldData)
    {
        // Prevent endlessTerrain from trying to load deleted chunks
        EndlessTerrain.terrainChunkDictonary.Clear();
        EndlessTerrain.terrainChunksVisibleLastUpdate.Clear();

        MapGenerator.Instance.terrainSettings = worldData.terrainSettings;

        this.worldData = worldData;
        chunks = worldData.terrainData.chunks;
        sectors = new();

        Debug.Log($"Loaded {chunks.Count} chunks");

    }
    public void SaveData(WorldData worldData)
    {
        worldData.terrainData.chunks = (SerializableDictonary<Vector2, ChunkData>)chunks;

        Debug.Log($"Saved {worldData.terrainData.chunks.Count} chunks");
    }

    // EndlessTerrain uses this to add chunks
    public void AddChunk(Vector2 center, VertexData[,] map)
    {
        List<ListWrapper<VertexData>> tempMap = new();
        for (int x = 0; x < map.GetLength(0); x++)
        {
            ListWrapper<VertexData> temp = new();
            for (int y = 0; y < map.GetLength(1); y++)
            {
                temp.list.Add(map[x, y]);
            }
            tempMap.Add(temp);
        }
        chunks[center] = new(tempMap);
    }
}

