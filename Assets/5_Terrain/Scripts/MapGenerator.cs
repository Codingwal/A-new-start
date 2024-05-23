using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

public class MapGenerator : Singleton<MapGenerator>
{
    // This is set by the MapDataHandler script
    [HideInInspector] public TerrainSettings terrainSettings;
    public TextureData textureData;

    public Material terrainMaterial;

    public const int mapChunkSize = 241;

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
        UpdateTextures();
    }

    public void UpdateTextures()
    {
        Debug.Log("Reloading textures");

        textureData.UpdateMeshHeights(terrainMaterial, terrainSettings.minHeight, terrainSettings.maxHeight);
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };
        new Thread(threadStart).Start();
    }
    void MapDataThread(Vector2 center, Action<MapData> callback)
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
        MeshData meshData = MeshGenerator.GenerateTerrainMesh
        (mapData.heightMap, terrainSettings.meshHeightMultiplier, lod);
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
    MapData GenerateMapData(Vector2 center)
    {
        // Debug.Log("Generating MapData");

        MapDataHandler mapDataHandler = MapDataHandler.Instance;

        if (mapDataHandler.chunks.ContainsKey(center))
        {
            return new(mapDataHandler.chunks[center].heightMap);
        }

        float[,] noiseMap = Noise.GenerateNoiseMap
        (mapChunkSize, mapChunkSize, MapDataHandler.Instance.worldData.terrainData.seed, terrainSettings.noiseScale, terrainSettings.octaves,
        terrainSettings.persistance, terrainSettings.lacunarity, terrainSettings.slopeImpact, center);

        GenerateRivers(noiseMap, terrainSettings.minWaterSourceHeight);

        mapDataHandler.AddChunk(center, noiseMap);

        return new(noiseMap);
    }
    struct VertexWaterInfo
    {
        float amount;
        Vector2 velocity;
    }
    // void GenerateRivers2(float[,] noiseMap, float minWaterSourceHeight)
    // {
    //     // parameters
    //     int mapWidth = noiseMap.GetLength(0);
    //     int mapHeight = noiseMap.GetLength(1);
    //     VertexWaterInfo[,] riverMap = new VertexWaterInfo[mapWidth, mapHeight];

    //     // Try to generate a water source at a random position
    //     System.Random rnd = new();
    //     Vector2Int pos = new(rnd.Next(1, mapWidth - 1), rnd.Next(1, mapHeight - 1));
    //     float value = noiseMap[pos.x, pos.y];


    // }
    void GenerateRivers(float[,] noiseMap, float minWaterSourceHeight)
    {
        // parameters
        int mapWidth = noiseMap.GetLength(0);
        int mapHeight = noiseMap.GetLength(1);
        float[,] riverMap = new float[mapWidth, mapHeight];

        // Try to generate a water source at a random position
        System.Random rnd = new();
        Vector2Int pos = new(rnd.Next(1, mapWidth - 1), rnd.Next(1, mapHeight - 1));
        float value = noiseMap[pos.x, pos.y];

        // if a water source can be spawned at that point...
        // if (value > minWaterSourceHeight)
        if (true)
        {
            AddIndent(riverMap, 3, pos);

            int i = 0;
            while (true)
            {
                // get direction and height of lowest adjacent vertex
                float lowestNeighbourHeight = float.PositiveInfinity;
                Vector2 lowestNeighbourOffset = new();

                if (noiseMap[pos.x + 1, pos.y] < lowestNeighbourHeight)
                {
                    lowestNeighbourHeight = noiseMap[pos.x + 1, pos.y];
                    lowestNeighbourOffset = new(1, 0);
                }
                if (noiseMap[pos.x - 1, pos.y] < lowestNeighbourHeight)
                {
                    lowestNeighbourHeight = noiseMap[pos.x - 1, pos.y];
                    lowestNeighbourOffset = new(-1, 0);
                }
                if (noiseMap[pos.x, pos.y + 1] < lowestNeighbourHeight)
                {
                    lowestNeighbourHeight = noiseMap[pos.x, pos.y + 1];
                    lowestNeighbourOffset = new(0, 1);
                }
                if (noiseMap[pos.x, pos.y - 1] < lowestNeighbourHeight)
                {
                    lowestNeighbourHeight = noiseMap[pos.x, pos.y - 1];
                    lowestNeighbourOffset = new(0, -1);
                }

                // lowest neighbour is the next vertex
                value = lowestNeighbourHeight;
                pos += lowestNeighbourOffset;

                if (pos.x < 1 || pos.x + 1 >= mapWidth || pos.y < 1 || pos.y + 1 >= mapHeight)
                    break;

                AddIndent(riverMap, 2, pos);

                i++;
                if (i > 50)
                {
                    break;
                }
            }


            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] -= riverMap[x, y] / 50;
                }
            }
        }
    }
    void AddIndent(float[,] map, float strength, Vector2 pos)
    {
        for (int y = -5; y <= 5; y++)
        {
            for (int x = -5; x <= 5; x++)
            {
                float distance = Mathf.Clamp(Mathf.Sqrt(x * x + y * y), 1, 100);
                int px = x + (int)pos.x;
                int py = y + (int)pos.y;

                if (!(px < 0 || px >= map.GetLength(0) || py < 0 || py >= map.GetLength(1)))
                {
                    if (map[px, py] < strength - 0.5f * distance)
                        map[px, py] = strength - 0.5f * distance;
                }
            }
        }
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
    public float[,] heightMap { get; private set; }

    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }

    public MapData(List<ListWrapper<float>> heightMapList)
    {
        heightMap = new float[MapGenerator.mapChunkSize, MapGenerator.mapChunkSize];

        for (int x = 0; x < MapGenerator.mapChunkSize; x++)
        {
            for (int y = 0; y < MapGenerator.mapChunkSize; y++)
            {
                heightMap[x, y] = heightMapList[x].list[y];
            }
        }
    }
}