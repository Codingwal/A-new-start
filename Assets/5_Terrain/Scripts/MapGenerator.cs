using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using System.Collections.Concurrent;

public class MapGenerator : Singleton<MapGenerator>
{
    // This is set by the MapDataHandler script
    [HideInInspector] public TerrainSettings terrainSettings;

    public Material terrainMaterial;
    readonly int[] possibleMapChunkSizes = { 121, 241 };
    [Dropdown("possibleMapChunkSizes")]
    public int mapChunkSize = 241;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new();
    ConcurrentDictionary<Vector2Int, List<VertexToCalcInfo>> transferredWaterDict = new();

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
        MeshData meshData = MeshGenerator.GenerateTerrainMesh
        (mapData.map, terrainSettings.meshHeightMultiplier, lod);
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
    MapData GenerateMapData(Vector2Int center)
    {
        // Debug.Log("Generating MapData");

        MapDataHandler mapDataHandler = MapDataHandler.Instance;

        if (mapDataHandler.chunks.ContainsKey(center))
        {
            return new(mapDataHandler.chunks[center].map);
        }

        float[,] noiseMap = Noise.GenerateNoiseMap
        (mapChunkSize, mapChunkSize, MapDataHandler.Instance.worldData.terrainData.seed, terrainSettings.noiseScale, terrainSettings.octaves,
        terrainSettings.persistance, terrainSettings.lacunarity, terrainSettings.slopeImpact, terrainSettings.meshHeightMultiplier, center);

        VertexData[,] map = new VertexData[mapChunkSize, mapChunkSize];

        List<VertexToCalcInfo> transferredWater = new();
        if (transferredWaterDict.ContainsKey(center))
            transferredWater = transferredWaterDict[center];

        for (int x = 0; x < mapChunkSize; x++)
        {
            for (int y = 0; y < mapChunkSize; y++)
            {
                map[x, y].height = noiseMap[x, y];
            }
        }

        GenerateRivers(map, transferredWater, center / mapChunkSize, MapDataHandler.Instance.worldData.terrainData.seed, terrainSettings.minWaterSourceHeight, 1);

        mapDataHandler.AddChunk(center, map);

        return new(map);
    }
    struct VertexToCalcInfo
    {
        public Vector2Int pos;
        public VertexData source;
        public float sourceInfluence;
        public VertexToCalcInfo(Vector2Int pos, VertexData source, float sourceInfluence)
        {
            this.pos = pos;
            this.source = source;
            this.sourceInfluence = sourceInfluence;
        }
    }
    void GenerateRivers(VertexData[,] map, List<VertexToCalcInfo> transferredWater, Vector2Int center, int seed, float minWaterSourceHeight, float waterSlopeSpeedImpact)
    {
        // Temp (garantuees a water source in every chunk)
        minWaterSourceHeight = -1;
        float slopeTolerance = 1;

        // parameters
        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);

        Queue<VertexToCalcInfo> verticesToCalc = new();

        // Try to generate a water source at a random position
        {
            System.Random rnd = new(seed);
            // Vector2Int pos = new(rnd.Next(1, mapWidth - 1), rnd.Next(1, mapHeight - 1));
            Vector2Int pos = new(mapWidth / 2, mapHeight / 2);
            float value = map[pos.x, pos.y].height;

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
                            verticesToCalc.Enqueue(new(new(pos.x + x, pos.y + y), new(map[pos.x, pos.y].height, 1, new()), 1));
                        }
                    }
                }
            }
        }
        foreach (VertexToCalcInfo vertexToCalcInfo in transferredWater)
        {
            verticesToCalc.Enqueue(vertexToCalcInfo);
        }

        int i = 0;
        while (verticesToCalc.Count > 0)
        {
            // Get next vertex
            VertexToCalcInfo info = verticesToCalc.Dequeue();
            Vector2Int pos = info.pos;

            if (map[pos.x, pos.y].waterAmount != 0) continue;
            if (map[pos.x, pos.y].height == 0) continue;

            i++;
            if (i > 100000)
            {
                Debug.LogError("Limit reached!");
                break;
            }

            VertexData data = new();

            // Get the velocity and the water from the neighbour that functions as the source
            VertexData neighbour = info.source;

            data.waterAmount += neighbour.waterAmount * info.sourceInfluence;

            // Get the direction and the height of lowest adjacent vertex
            float lowestNeighbourHeight = float.PositiveInfinity;
            Vector2Int lowestNeighbourOffset = new();

            if (VertexHeightAtNeighbour(map, pos, new Vector2Int(1, 0)) < lowestNeighbourHeight)
            {
                lowestNeighbourHeight = VertexHeightAtNeighbour(map, pos, new Vector2Int(1, 0));
                lowestNeighbourOffset = new(1, 0);
            }
            if (VertexHeightAtNeighbour(map, pos, new Vector2Int(-1, 0)) < lowestNeighbourHeight)
            {
                lowestNeighbourHeight = VertexHeightAtNeighbour(map, pos, new Vector2Int(-1, 0));
                lowestNeighbourOffset = new(-1, 0);
            }
            if (VertexHeightAtNeighbour(map, pos, new Vector2Int(0, 1)) < lowestNeighbourHeight)
            {
                lowestNeighbourHeight = VertexHeightAtNeighbour(map, pos, new Vector2Int(0, 1));
                lowestNeighbourOffset = new(0, 1);
            }
            if (VertexHeightAtNeighbour(map, pos, new Vector2Int(0, -1)) < lowestNeighbourHeight)
            {
                lowestNeighbourHeight = VertexHeightAtNeighbour(map, pos, new Vector2Int(0, -1));
                lowestNeighbourOffset = new(0, -1);
            }

            // Calculate the slope from this vertex to the lowest adjacent vertex
            float slope = VertexHeightAtNeighbour(map, pos, new(0, 0)) - lowestNeighbourHeight;

            // If the lowest adjacent vertex is higher, stop the generation
            if (slope < -slopeTolerance)
            {
                map[pos.x, pos.y] = new(map[pos.x, pos.y].height, data.waterAmount, new());
                Debug.LogWarning("Too flat");
                continue;
            }

            // add the acceleration because of the slope, set magnitude to the calculated speed (the speed also depends on the slope)
            float speed = slope * waterSlopeSpeedImpact;

            // store the data in the array
            map[pos.x, pos.y] = map[pos.x, pos.y] + data;

            // Enqueue all effected vertices

            Vector2Int affectedVertexPos = new(pos.x + lowestNeighbourOffset.x, pos.y);
            if (affectedVertexPos.x >= 0)
            {
                if (affectedVertexPos.x < mapWidth)
                {
                    if (map[affectedVertexPos.x, affectedVertexPos.y].waterAmount == 0)
                        verticesToCalc.Enqueue(new(affectedVertexPos, data, 1));
                }
                else
                {
                    if (!transferredWaterDict.ContainsKey(center + Vector2Int.right))
                        transferredWaterDict[center + Vector2Int.right] = new();
                    transferredWaterDict[center + Vector2Int.right].Add(new(new(0, affectedVertexPos.y), data, 1));
                }
            }
            else
            {
                if (!transferredWaterDict.ContainsKey(center + Vector2Int.left))
                    transferredWaterDict[center + Vector2Int.left] = new();
                transferredWaterDict[center + Vector2Int.left].Add(new(new(map.GetLength(0) - 1, affectedVertexPos.y), data, 1));
            }

            affectedVertexPos = new(pos.x, pos.y + lowestNeighbourOffset.y);
            if (affectedVertexPos.y >= 0)
            {
                if (affectedVertexPos.y < mapHeight)
                {
                    if (map[affectedVertexPos.x, affectedVertexPos.y].waterAmount == 0)
                        verticesToCalc.Enqueue(new(affectedVertexPos, data, 1));
                }
                else
                {
                    if (!transferredWaterDict.ContainsKey(center + Vector2Int.up))
                        transferredWaterDict[center + Vector2Int.up] = new();
                    transferredWaterDict[center + Vector2Int.up].Add(new(new(affectedVertexPos.x, 0), data, 1));
                }
            }
            else
            {
                if (!transferredWaterDict.ContainsKey(center + Vector2Int.down))
                    transferredWaterDict[center + Vector2Int.down] = new();
                transferredWaterDict[center + Vector2Int.down].Add(new(new(affectedVertexPos.x, map.GetLength(0) - 1), data, 1));
            }


        }

        i = 0;
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                map[x, y].height -= map[x, y].waterAmount * 0.7f;
                if (map[x, y].waterAmount > 0)
                {
                    i++;
                }
            }
        }
        Debug.Log($"Water vertices count: {i}");
    }
    float VertexHeightAtNeighbour(VertexData[,] map, Vector2Int pos, Vector2Int offset)
    {
        if (pos.x + offset.x < map.GetLength(0) && pos.x + offset.x >= 0 && pos.y + offset.y < map.GetLength(1) && pos.y + offset.y >= 0)
        {
            VertexData vertexData = map[pos.x + offset.x, pos.y + offset.y];
            return vertexData.height + vertexData.waterAmount * 10;
        }
        else
        {
            // Estimate the position by continuing the slope of the vertex in the opposite direction
            return 2 * map[pos.x, pos.y].height - map[pos.x - offset.x, pos.y - offset.y].height;
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
    public VertexData[,] map { get; private set; }

    public MapData(VertexData[,] map)
    {
        this.map = map;
    }

    public MapData(List<ListWrapper<VertexData>> map)
    {
        this.map = new VertexData[MapGenerator.Instance.mapChunkSize, MapGenerator.Instance.mapChunkSize];
        for (int x = 0; x < MapGenerator.Instance.mapChunkSize; x++)
        {
            for (int y = 0; y < MapGenerator.Instance.mapChunkSize; y++)
            {
                this.map[x, y] = map[x].list[y];
            }
        }
    }
}