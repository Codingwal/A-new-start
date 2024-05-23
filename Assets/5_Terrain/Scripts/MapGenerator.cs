using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using UnityEditor.Search;

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

        GenerateRivers2(noiseMap, terrainSettings.minWaterSourceHeight, 100);

        mapDataHandler.AddChunk(center, noiseMap);

        return new(noiseMap);
    }
    struct VertexWaterInfo
    {
        public float amount;
        public Vector2 velocity;
        public VertexWaterInfo(float amount, Vector2 velocity)
        {
            this.amount = amount;
            this.velocity = velocity;
        }
        public static VertexWaterInfo operator +(VertexWaterInfo a, VertexWaterInfo b)
        {
            return new(a.amount + b.amount, a.velocity + b.velocity);
        }
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
    void GenerateRivers2(float[,] noiseMap, float minWaterSourceHeight, float waterSlopeSpeedImpact)
    {
        // Temp (garantuees a water source in every chunk)
        minWaterSourceHeight = -1;

        // parameters
        int mapWidth = noiseMap.GetLength(0);
        int mapHeight = noiseMap.GetLength(1);
        VertexWaterInfo[,] riverMap = new VertexWaterInfo[mapWidth, mapHeight];

        Queue<VertexToCalcInfo> verticesToCalc = new();

        // Try to generate a water source at a random position
        {
            System.Random rnd = new();
            Vector2Int pos = new(rnd.Next(1, mapWidth - 1), rnd.Next(1, mapHeight - 1));
            float value = noiseMap[pos.x, pos.y];

            // Only generate if all requirements are met
            if (value > minWaterSourceHeight)
            {
                int t = 0;
                // 5 * 5 area with the pos in the center
                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        // 25% chance
                        if (rnd.Next(1) == 0)
                        {
                            t++;
                            // generate source
                            verticesToCalc.Enqueue(new(new(pos.x + x, pos.y + y), new(1, new()), 1));
                        }
                    }
                }
                Debug.Log($"Watersource count: {t}");
            }
        }

        int i = 0;
        while (verticesToCalc.Count > 0)
        {
            i++;
            if (i > 1000)
            {
                break;
            }

            // Get next vertex
            VertexToCalcInfo info = verticesToCalc.Dequeue();
            Vector2Int pos = info.pos;

            VertexWaterInfo data = new();

            // Get the velocity and the water from the neighbour that functions as the source
            VertexWaterInfo neighbour = info.source;

            data.amount += neighbour.amount * info.sourceInfluence;
            data.velocity += neighbour.velocity;

            // Get the direction and the height of lowest adjacent vertex
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

            // Calculate the slope from this vertex to the lowest adjacent vertex
            float slope = noiseMap[pos.x, pos.y] - lowestNeighbourHeight;

            // If the lowest adjacent vertex is higher, stop the generation
            if (slope < 0)
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
                Vector2Int affectedVertexPos = new(pos.x + Mathf.CeilToInt(Mathf.Clamp01(data.velocity.x)), pos.y);
                if (affectedVertexPos.x > 0 && affectedVertexPos.x + 1 < mapWidth && affectedVertexPos.y > 0 && affectedVertexPos.y + 1 < mapHeight)
                    verticesToCalc.Enqueue(new(affectedVertexPos, data, data.velocity.normalized.x));
            }

            if (Mathf.Abs(data.velocity.y) > 0.01)
            {
                Vector2Int affectedVertexPos = new(pos.x, pos.y + Mathf.CeilToInt(Mathf.Clamp01(data.velocity.y)));
                if (affectedVertexPos.x > 0 && affectedVertexPos.x + 1 < mapWidth && affectedVertexPos.y > 0 && affectedVertexPos.y + 1 < mapHeight)
                    verticesToCalc.Enqueue(new(affectedVertexPos, data, data.velocity.normalized.y));
            }
        }

        int c = 0;
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] -= riverMap[x, y].amount / 50;

                if (riverMap[x, y].amount != 0)
                {
                    c++;
                }
            }
        }
        Debug.Log($"WaterVertices: {c}");
    }
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
                Vector2Int lowestNeighbourOffset = new();

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