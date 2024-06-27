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

        if (worldData.terrainData.seed == 0)
            worldData.terrainData.seed = GenerateSeed(worldData.terrainSettings);

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
    public void AddChunk(Vector2 center, VertexData[,] map, List<List<Vector3>> rivers)
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
        List<ListWrapper<Vector3>> tempRivers = new();
        foreach (List<Vector3> river in rivers)
        {
            tempRivers.Add(new(river));
        }
        chunks[center] = new(tempMap, tempRivers);
    }
    int GenerateSeed(TerrainSettings terrainSettings)
    {
        System.Random rnd = new();

        int seed = rnd.Next();
        int i = 1;
        while (VertexGenerator.GenerateVertexData(new Vector2(0, 0), seed, terrainSettings, terrainSettings.terrainScale) < 20)
        {
            seed = rnd.Next();
            i++;
        }
        Debug.Log($"Generated seed: {seed} with {i} tries");
        return seed;
    }
}

