using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Serializer
{
    static BinaryWriter bw;
    public static void Serialize(FileStream fs, WorldData data)
    {
        bw = new(fs);

        Write(data.playerSettings);
        Write(data.playerData);
        Write(data.terrainSettings);
        Write(data.terrainData);
    }
    static void Write<T>(T data)
    {
        if (data is VertexData)
            Write((VertexData)(object)data);
        else if (data is Biome)
            Write((Biome)(object)data);
        else if (data is BiomeTreeType)
            Write((BiomeTreeType)(object)data);
        else if (data is Vector2)
            Write((Vector2)(object)data);
        else if (data is ChunkData)
            Write((ChunkData)(object)data);
        else if (data is TreeData)
            Write((TreeData)(object)data);
        else
            Debug.LogError($"ERROR SAVING DATA: No overload for type {typeof(T)} found!");
    }
    static void Write<T>(List<T> data)
    {
        bw.Write(data.Count);
        foreach (T element in data)
        {
            Write(element);
        }
    }
    static void Write<TKey, TValue>(Dictionary<TKey, TValue> data)
    {
        bw.Write(data.Count);
        foreach (KeyValuePair<TKey, TValue> pair in data)
        {
            Write(pair.Key);
            Write(pair.Value);
        }
    }
    static void Write<T>(T[,] data)
    {
        bw.Write(data.GetLength(0));
        bw.Write(data.GetLength(1));
        for (int x = 0; x < data.GetLength(0); x++)
        {
            for (int y = 0; y < data.GetLength(1); y++)
            {
                Write(data[x, y]);
            }
        }
    }
    static void Write(PlayerSettings data)
    {
    }
    static void Write(PlayerData data)
    {
        Write(data.position);
        Write(data.rotation);
    }
    static void Write(TerrainSettings data)
    {
        Write(data.biomes);
        bw.Write(data.biomeScale);

        bw.Write(data.uniformScale);
        bw.Write(data.terrainScale);

        bw.Write(data.noiseScale);
        bw.Write(data.octaves);
        bw.Write(data.octaveFrequencyFactor);
        bw.Write(data.octaveAmplitudeFactor);

        bw.Write(data.generateRivers);
        bw.Write(data.minWaterSourceHeight);
        bw.Write(data.riverWaterGain);
        bw.Write(data.maxRiverCount);
        bw.Write(data.maxRiverGenerationTries);
        bw.Write(data.minRiverSlope);
        bw.Write(data.riverDirectionImpact);

    }
    static void Write(TerrainData data)
    {
        bw.Write(data.seed);
        Write(data.chunks);
    }
    static void Write(Biome data)
    {
        bw.Write((uint)data.name);
        Write(data.bounds);
        Write(data.biomeSettings);
    }
    static void Write(BiomeSettings data)
    {
        bw.Write(data.heightMultiplier);
        bw.Write(data.slopeImpact);
        bw.Write(data.heightOffset);
        bw.Write(data.minTreeSpacing);
        Write(data.trees);
    }
    static void Write(BiomeTreeType data)
    {
        bw.Write(data.chance);
        Write(data.treeType);
    }
    static void Write(TreeType data)
    {
        bw.Write((uint)data.tree);
        bw.Write(data.minDistance);
    }
    static void Write(ChunkData data)
    {
        Write(data.bottomLeft);
        Write(data.bottomRight);
        Write(data.topLeft);
        Write(data.topRight);

        Write(data.map);
        Write(data.rivers);
        Write(data.trees);
    }
    static void Write(TreeData data)
    {
        Write(data.pos);
        bw.Write((uint)data.type);
    }
    static void Write(VertexData data)
    {
        bw.Write(data.height);
        bw.Write(data.waterAmount);
    }
    static void Write(Bounds data)
    {
        Write(data.center);
        Write(data.extents);
    }
    static void Write(Vector3 data)
    {
        bw.Write(data.x);
        bw.Write(data.y);
        bw.Write(data.z);
    }
    static void Write(Vector2 data)
    {
        bw.Write(data.x);
        bw.Write(data.y);
    }
}