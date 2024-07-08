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

        chunks = new ConcurrentDictionary<Vector2, ChunkData>(worldData.terrainData.chunks);
        sectors = new();

        Debug.Log($"Loaded {chunks.Count} chunks");
    }
    public void SaveData(WorldData _worldData)
    {
        _worldData.terrainData.chunks = new Dictionary<Vector2, ChunkData>(chunks);
        _worldData.terrainData.seed = worldData.terrainData.seed;

        Debug.Log($"Saved {_worldData.terrainData.chunks.Count} chunks");
    }

    // EndlessTerrain uses this to add chunks
    public void AddChunk(Vector2 center, ChunkData mapData)
    {
        chunks[center] = mapData;
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

