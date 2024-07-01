using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

public class MapGenerator : Singleton<MapGenerator>
{
    // This is set by the MapDataHandler script
    [HideInInspector] public TerrainSettings terrainSettings;

    public Material terrainMaterial;
    public int chunkSize = 241;
    public int chunksPerSector1D = 9;
    public int vertexIncrement = 1;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new();
    protected override void SingletonAwake()
    {
        MainSystem.LoadWorld += LoadWorld;
    }
    private void OnDisable()
    {
        MainSystem.LoadWorld -= LoadWorld;
    }
    private void LoadWorld()
    {
        // Update the map

    }

    public void RequestMapData(Vector2Int center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };
        new Thread(threadStart).Start();
    }
    void MapDataThread(Vector2Int center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new(callback, mapData));
        }
    }
    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }
    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.map, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callBack(threadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callBack(threadInfo.parameter);
            }
        }
    }
    private MapData GenerateMapData(Vector2Int center)
    {
        MapDataHandler mapDataHandler = MapDataHandler.Instance;

        if (MapDataHandler.chunks.ContainsKey(center))
        {
            return new(MapDataHandler.chunks[center].map, MapDataHandler.chunks[center].rivers);
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
        MapData map = MapDataGenerator.GenerateMapData(center, chunkSize, MapDataHandler.worldData.terrainData.seed, terrainSettings, sectorData, vertexIncrement);

        mapDataHandler.AddChunk(center, map.map, map.rivers);

        return map;
    }
    struct MapThreadInfo<T>
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
public struct MapData
{
    public VertexData[,] map { get; private set; }
    public List<List<Vector3>> rivers;

    public MapData(VertexData[,] map, List<List<Vector3>> rivers)
    {
        this.map = map;
        this.rivers = rivers;
    }

    public MapData(List<ListWrapper<VertexData>> map, List<ListWrapper<Vector3>> rivers)
    {
        this.map = new VertexData[MapGenerator.Instance.chunkSize, MapGenerator.Instance.chunkSize];
        for (int x = 0; x < MapGenerator.Instance.chunkSize; x++)
        {
            for (int y = 0; y < MapGenerator.Instance.chunkSize; y++)
            {
                this.map[x, y] = map[x].list[y];
            }
        }

        this.rivers = new();
        foreach (ListWrapper<Vector3> river in rivers)
        {
            this.rivers.Add(river.list);
        }
    }
}