using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// The MapGenerator handles threading for the terrainData generation and calls the different functions needed to generate the terrain in the GenerateMapData function
/// </summary>
public class MapGenerator : Singleton<MapGenerator>
{
    // This is set by the MapDataHandler script
    [HideInInspector] public TerrainSettings terrainSettings;

    public Material terrainMaterial;
    public int chunkSize;
    public int chunksPerSector1D;
    public int vertexIncrement;

    readonly Queue<MapThreadInfo<ChunkData>> mapDataThreadInfoQueue = new();
    readonly Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new();

    public void RequestMapData(Vector2Int center, Action<ChunkData> callback)
    {
        void threadStart()
        {
            MapDataThread(center, callback);
        }
        new Thread(threadStart).Start();
    }
    void MapDataThread(Vector2Int center, Action<ChunkData> callback)
    {
        ChunkData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new(callback, mapData));
        }
    }
    public void RequestMeshData(ChunkData mapData, int lod, Action<MeshData> callback)
    {
        void threadStart()
        {
            MeshDataThread(mapData, lod, callback);
        }
        new Thread(threadStart).Start();
    }
    void MeshDataThread(ChunkData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.map, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
        {
            MapThreadInfo<ChunkData> threadInfo = mapDataThreadInfoQueue.Dequeue();
            threadInfo.callBack(threadInfo.parameter);
        }

        for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
        {
            MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
            threadInfo.callBack(threadInfo.parameter);
        }

    }
    private ChunkData GenerateMapData(Vector2Int center)
    {
        MapDataHandler mapDataHandler = MapDataHandler.Instance;

        if (MapDataHandler.chunks.ContainsKey(center))
        {
            return MapDataHandler.chunks[center];
        }
        int sectorSize = chunksPerSector1D * chunkSize;

        Vector2Int sectorCenter = Vector2Int.RoundToInt(center / sectorSize) * sectorSize;

        SectorData sectorData;
        lock (mapDataHandler.sectors)
        {
            if (mapDataHandler.sectors.ContainsKey(sectorCenter))
            {
                sectorData = mapDataHandler.sectors[sectorCenter];
            }
            else
            {
                if (terrainSettings.generateRivers)
                    sectorData = RiverGenerator.GenerateRivers(sectorCenter, MapDataHandler.worldData.terrainData.seed, sectorSize, terrainSettings);
                else
                    sectorData = new();

                mapDataHandler.sectors[sectorCenter] = sectorData;
            }
        }
        ChunkData map = MapDataGenerator.GenerateMapData(center, chunkSize, MapDataHandler.worldData.terrainData.seed, terrainSettings, sectorData, vertexIncrement);
        TreeDataGenerator.GenerateTrees(map, terrainSettings, MapDataHandler.worldData.terrainData.seed, center, chunkSize);

        mapDataHandler.AddChunk(center, map);

        return map;
    }

    readonly struct MapThreadInfo<T>
    {
        public readonly Action<T> callBack;
        public readonly T parameter;
        public MapThreadInfo(Action<T> callBack, T parameter)
        {
            this.callBack = callBack;
            this.parameter = parameter;
        }
    }

}