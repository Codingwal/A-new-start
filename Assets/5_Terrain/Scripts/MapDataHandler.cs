using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class MapDataHandler : Singleton<MapDataHandler>, IDataCallbackReceiver
{
    // The chunk dictionary
    public static ConcurrentDictionary<Vector2, ChunkData> chunks;
    public ConcurrentDictionary<Vector2, SectorData> sectors;
    public ConcurrentDictionary<Vector2, Vector2[]> chunksWaitingForSector;
    public static WorldData worldData;

    public void LoadData(WorldData _worldData)
    {
        worldData = _worldData;

        // Prevent endlessTerrain from trying to load deleted chunks
        EndlessTerrain.terrainChunkDictonary.Clear();
        EndlessTerrain.terrainChunksVisibleLastUpdate.Clear();

        MapGenerator.Instance.terrainSettings = worldData.terrainSettings;

        if (worldData.terrainData.seed == 0)
            worldData.terrainData.seed = GenerateSeed(worldData.terrainSettings);

        chunks = worldData.terrainData.chunks;
        sectors = new();

        Debug.Log($"Loaded {chunks.Count} chunks");
    }
    public void SaveData(WorldData _worldData)
    {
        _worldData.terrainData.chunks = (SerializableDictonary<Vector2, ChunkData>)chunks;
        _worldData.terrainData.seed = worldData.terrainData.seed;

        Debug.Log($"Saved {_worldData.terrainData.chunks.Count} chunks");
    }

    // EndlessTerrain uses this to add chunks
    public void AddChunk(Vector2 center, MapData mapData)
    {
        ChunkData chunkData = new();

        List<ListWrapper<VertexData>> map = new();
        for (int x = 0; x < mapData.map.GetLength(0); x++)
        {
            ListWrapper<VertexData> temp = new();
            for (int y = 0; y < mapData.map.GetLength(1); y++)
            {
                temp.list.Add(mapData.map[x, y]);
            }
            map.Add(temp);
        }
        chunkData.map = map;

        List<ListWrapper<Vector3>> rivers = new();
        foreach (List<Vector3> river in mapData.rivers)
        {
            rivers.Add(new(river));
        }
        chunkData.rivers = rivers;

        chunkData.trees = mapData.trees;

        chunkData.bottomLeft = mapData.bottomLeft;
        chunkData.bottomRight = mapData.bottomRight;
        chunkData.topLeft = mapData.topLeft;
        chunkData.topRight = mapData.topRight;

        chunks[center] = chunkData;
    }
    int GenerateSeed(TerrainSettings terrainSettings)
    {
        System.Random rnd = new();

        int seed = rnd.Next(-100000, 100000);
        int i = 1;
        while (VertexGenerator.GenerateVertexData(new Vector2(0, 0), seed, terrainSettings, terrainSettings.terrainScale) < 20)
        {
            // Set limit so that it's easier for players to share and remember seeds
            seed = rnd.Next(-100000, 100000);
            i++;

            if (i > 100)
            {
                Debug.LogError("Can't find seed");
                seed = 0;
                break;
            }
        }
        Debug.Log($"Generated seed: {seed} with {i} tries");
        return seed;
    }
}

