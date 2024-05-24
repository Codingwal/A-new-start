using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using UnityEditor.Search;

public class MapGenerator : Singleton<MapGenerator>
{
    // This is set by the MapDataHandler script
    [HideInInspector] public TerrainSettings terrainSettings;

    public Material terrainMaterial;

    readonly int[] possibleMapChunkSizes = {121, 241};
    [Dropdown("possibleMapChunkSizes")]
    public int mapChunkSize = 241;

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
            return new(mapDataHandler.chunks[center].heightMap, mapDataHandler.chunks[center].riverMap);
        }

        float[,] noiseMap = Noise.GenerateNoiseMap
        (mapChunkSize, mapChunkSize, MapDataHandler.Instance.worldData.terrainData.seed, terrainSettings.noiseScale, terrainSettings.octaves,
        terrainSettings.persistance, terrainSettings.lacunarity, terrainSettings.slopeImpact, terrainSettings.meshHeightMultiplier, center);

        VertexWaterInfo[,] riverMap = GenerateRivers(noiseMap, MapDataHandler.Instance.worldData.terrainData.seed, terrainSettings.minWaterSourceHeight, 1);

        mapDataHandler.AddChunk(center, noiseMap, riverMap);

        return new(noiseMap, riverMap);
    }
    struct VertexToCalcInfo
    {
        public Vector2Int pos;
        public VertexWaterInfo source;
        public float sourceInfluence;
        public VertexToCalcInfo(Vector2Int pos, VertexWaterInfo source, float sourceInfluence)
        {
            this.pos = pos;
            this.source = source;
            this.sourceInfluence = sourceInfluence;
        }
    }
    VertexWaterInfo[,] GenerateRivers(float[,] noiseMap, int seed, float minWaterSourceHeight, float waterSlopeSpeedImpact)
    {
        // Temp (garantuees a water source in every chunk)
        minWaterSourceHeight = -1;
        float slopeTolerance = 1;

        // parameters
        int mapWidth = noiseMap.GetLength(0);
        int mapHeight = noiseMap.GetLength(1);
        VertexWaterInfo[,] riverMap = new VertexWaterInfo[mapWidth, mapHeight];

        Queue<VertexToCalcInfo> verticesToCalc = new();

        // Try to generate a water source at a random position
        {
            System.Random rnd = new(seed);
            // Vector2Int pos = new(rnd.Next(1, mapWidth - 1), rnd.Next(1, mapHeight - 1));
            Vector2Int pos = new(mapWidth / 2, mapHeight / 2);
            float value = noiseMap[pos.x, pos.y];

            // Only generate if all requirements are met
            if (value > minWaterSourceHeight)
            {
                // 5 * 5 area with the pos in the center
                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        // 100% chance
                        if (rnd.Next(1) == 0)
                        {
                            // generate source
                            verticesToCalc.Enqueue(new(new(pos.x + x, pos.y + y), new(1, new()), 1));
                        }
                    }
                }
            }
        }

        int i = 0;
        while (verticesToCalc.Count > 0)
        {
            // Get next vertex
            VertexToCalcInfo info = verticesToCalc.Dequeue();
            Vector2Int pos = info.pos;

            if (riverMap[pos.x, pos.y].amount != 0) continue;

            i++;
            if (i > 100000)
            {
                Debug.LogError("Limit reached!");
                break;
            }

            VertexWaterInfo data = new();

            // Get the velocity and the water from the neighbour that functions as the source
            VertexWaterInfo neighbour = info.source;

            data.amount += neighbour.amount * info.sourceInfluence;
            data.velocity += neighbour.velocity;

            // Get the direction and the height of lowest adjacent vertex
            float lowestNeighbourHeight = float.PositiveInfinity;
            Vector2 lowestNeighbourOffset = new();

            if (VertexHeight(noiseMap, riverMap, pos + new Vector2Int(1, 0)) < lowestNeighbourHeight)
            {
                lowestNeighbourHeight = noiseMap[pos.x + 1, pos.y];
                lowestNeighbourOffset = new(1, 0);
            }
            if (VertexHeight(noiseMap, riverMap, pos + new Vector2Int(-1, 0)) < lowestNeighbourHeight)
            {
                lowestNeighbourHeight = noiseMap[pos.x - 1, pos.y];
                lowestNeighbourOffset = new(-1, 0);
            }
            if (VertexHeight(noiseMap, riverMap, pos + new Vector2Int(0, 1)) < lowestNeighbourHeight)
            {
                lowestNeighbourHeight = noiseMap[pos.x, pos.y + 1];
                lowestNeighbourOffset = new(0, 1);
            }
            if (VertexHeight(noiseMap, riverMap, pos + new Vector2Int(0, -1)) < lowestNeighbourHeight)
            {
                lowestNeighbourHeight = noiseMap[pos.x, pos.y - 1];
                lowestNeighbourOffset = new(0, -1);
            }

            // Calculate the slope from this vertex to the lowest adjacent vertex
            float slope = noiseMap[pos.x, pos.y] - lowestNeighbourHeight;

            // If the lowest adjacent vertex is higher, stop the generation
            if (slope < -slopeTolerance)
            {
                riverMap[pos.x, pos.y] = new(data.amount, new());
                continue;
            }

            // add the acceleration because of the slope, set magnitude to the calculated speed (the speed also depends on the slope)
            float speed = slope * waterSlopeSpeedImpact;
            data.velocity = (data.velocity + speed * lowestNeighbourOffset).normalized * speed;

            // store the data in the array
            riverMap[pos.x, pos.y] = riverMap[pos.x, pos.y] + data;

            // Enqueue all effected vertices
            if (Mathf.Abs(data.velocity.x) > 0.01)
            {
                int roundedVelocity = (int)Mathf.Sign(data.velocity.x) * Mathf.CeilToInt(Math.Abs(data.velocity.x));
                Vector2Int affectedVertexPos = new(pos.x + Mathf.Clamp(roundedVelocity, -1, 1), pos.y);
                if (affectedVertexPos.x > 0 && affectedVertexPos.x + 1 < mapWidth && affectedVertexPos.y > 0 && affectedVertexPos.y + 1 < mapHeight)
                    if (riverMap[affectedVertexPos.x, affectedVertexPos.y].amount == 0)
                    {
                        verticesToCalc.Enqueue(new(affectedVertexPos, data, Mathf.Abs(data.velocity.normalized.x)));
                    }
            }
            if (Mathf.Abs(data.velocity.y) > 0.01)
            {
                int roundedVelocity = (int)Mathf.Sign(data.velocity.y) * Mathf.CeilToInt(Math.Abs(data.velocity.y));
                Vector2Int affectedVertexPos = new(pos.x, pos.y + Mathf.Clamp(roundedVelocity, -1, 1));
                if (affectedVertexPos.x > 0 && affectedVertexPos.x + 1 < mapWidth && affectedVertexPos.y > 0 && affectedVertexPos.y + 1 < mapHeight)
                    if (riverMap[affectedVertexPos.x, affectedVertexPos.y].amount == 0)
                    {
                        verticesToCalc.Enqueue(new(affectedVertexPos, data, Mathf.Abs(data.velocity.normalized.y)));
                    }
            }
        }

        i = 0;
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] -= riverMap[x, y].amount * 1.5f;
                if (riverMap[x, y].amount > 0)
                {
                    i++;
                }
            }
        }
        Debug.Log($"Water vertices count: {i}");

        return riverMap;
    }
    float VertexHeight(float[,] map, VertexWaterInfo[,] riverMap, Vector2Int pos)
    {
        return map[pos.x, pos.y] + riverMap[pos.x, pos.y].amount * 1;
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
    public VertexWaterInfo[,] riverMap;

    public MapData(float[,] heightMap, VertexWaterInfo[,] riverMap)
    {
        this.heightMap = heightMap;
        this.riverMap = riverMap;
    }

    public MapData(List<ListWrapper<float>> heightMapList, List<ListWrapper<VertexWaterInfo>> riverMapList)
    {
        heightMap = new float[MapGenerator.Instance.mapChunkSize, MapGenerator.Instance.mapChunkSize];
        for (int x = 0; x < MapGenerator.Instance.mapChunkSize; x++)
        {
            for (int y = 0; y < MapGenerator.Instance.mapChunkSize; y++)
            {
                heightMap[x, y] = heightMapList[x].list[y];
            }
        }

        riverMap = new VertexWaterInfo[MapGenerator.Instance.mapChunkSize, MapGenerator.Instance.mapChunkSize];
        for (int x = 0; x < MapGenerator.Instance.mapChunkSize; x++)
        {
            for (int y = 0; y < MapGenerator.Instance.mapChunkSize; y++)
            {
                riverMap[x, y] = riverMapList[x].list[y];
            }
        }
    }
}