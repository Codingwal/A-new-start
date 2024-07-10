using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public static class Deserializer
{
    static BinaryReader br;
    public static WorldData Deserialize(FileStream fs)
    {

        br = new(fs);

        WorldData data = new();

        Read(out data.playerSettings);
        Read(out data.playerData);
        Read(out data.terrainSettings);
        Read(out data.terrainData);


        return data;
    }
    static void Read<T>(out T data)
    {
        if (typeof(T) == typeof(VertexData))
        {
            Read(out VertexData tmp);
            data = (T)(object)tmp;
        }
        else if (typeof(T) == typeof(Biome))
        {
            Read(out Biome tmp);
            data = (T)(object)tmp;
        }
        else if (typeof(T) == typeof(BiomeTreeType))
        {
            Read(out BiomeTreeType tmp);
            data = (T)(object)tmp;
        }
        else if (typeof(T) == typeof(Vector2))
        {
            Read(out Vector2 tmp);
            data = (T)(object)tmp;
        }
        else if (typeof(T) == typeof(ChunkData))
        {
            Read(out ChunkData tmp);
            data = (T)(object)tmp;
        }
        else if (typeof(T) == typeof(TreeData))
        {
            Read(out TreeData tmp);
            data = (T)(object)tmp;
        }
        else
        {
            Debug.LogError($"ERROR LOADING DATA: No overload for type {typeof(T)} found!");
            data = default;
        }
    }
    static void Read<T>(out List<T> data)
    {
        data = new();
        int count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            Read(out T item);
            data.Add(item);
        }
    }
    static void Read<TKey, TValue>(out Dictionary<TKey, TValue> data)
    {
        data = new();
        int count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            Read(out TKey key);
            Read(out TValue value);
            data[key] = value;
        }
    }
    static void Read<T>(out T[,] data)
    {
        int lengthX = br.ReadInt32();
        int lengthY = br.ReadInt32();
        data = new T[lengthX, lengthY];
        for (int x = 0; x < lengthX; x++)
        {
            for (int y = 0; y < lengthY; y++)
            {
                Read(out T item);
                data[x, y] = item;
            }
        }
    }
    static void Read(out PlayerSettings data)
    {
        data = new();
    }
    static void Read(out PlayerData data)
    {
        data = new();
        Read(out data.position);
        Read(out data.rotation);
    }
    static void Read(out TerrainSettings data)
    {
        data = new();
        Read(out data.biomes);
        data.biomeScale = br.ReadSingle();

        data.uniformScale = br.ReadSingle();
        data.terrainScale = br.ReadSingle();

        data.noiseScale = br.ReadSingle();
        data.octaves = br.ReadInt32();
        data.octaveFrequencyFactor = br.ReadSingle();
        data.octaveAmplitudeFactor = br.ReadSingle();

        data.generateRivers = br.ReadBoolean();
        data.minWaterSourceHeight = br.ReadSingle();
        data.riverWaterGain = br.ReadSingle();
        data.maxRiverCount = br.ReadInt32();
        data.maxRiverGenerationTries = br.ReadInt32();
        data.minRiverSlope = br.ReadSingle();
        data.riverDirectionImpact = br.ReadSingle();
    }
    static void Read(out TerrainData data)
    {
        data = new()
        {
            seed = br.ReadInt32()
        };
        Read(out data.chunks);
    }
    static void Read(out Biome data)
    {
        data = new()
        {
            name = (Biomes)br.ReadUInt32()
        };
        Read(out data.bounds);
        Read(out data.biomeSettings);
    }
    static void Read(out BiomeSettings data)
    {
        data = new()
        {
            heightMultiplier = br.ReadSingle(),
            slopeImpact = br.ReadSingle(),
            heightOffset = br.ReadSingle(),
            minTreeSpacing = br.ReadSingle(),
        };
        Read(out data.trees);
    }
    static void Read(out BiomeTreeType data)
    {
        data = new()
        {
            chance = br.ReadSingle()
        };
        Read(out data.treeType);
    }
    static void Read(out TreeType data)
    {
        data = new()
        {
            tree = (TreeTypes)br.ReadUInt32(),
            minDistance = br.ReadSingle()
        };
    }
    static void Read(out ChunkData data)
    {
        data = new();
        Read(out data.bottomLeft);
        Read(out data.bottomRight);
        Read(out data.topLeft);
        Read(out data.topRight);

        Read(out data.map);
        Read(out data.rivers);
        Read(out data.trees);
    }
    static void Read(out TreeData data)
    {
        data = new();
        Read(out data.pos);
        data.type = (TreeTypes)br.ReadUInt32();
    }
    static void Read(out VertexData data)
    {
        data = new()
        {
            height = br.ReadSingle(),
            waterAmount = br.ReadSingle()
        };
    }
    static void Read(out Bounds data)
    {
        Read(out Vector3 center);
        Read(out Vector3 extents);
        data = new()
        {
            center = center,
            extents = extents
        };
    }
    static void Read(out Vector3 data)
    {
        data = new()
        {
            x = br.ReadSingle(),
            y = br.ReadSingle(),
            z = br.ReadSingle(),
        };
    }
    static void Read(out Vector2 data)
    {
        data = new()
        {
            x = br.ReadSingle(),
            y = br.ReadSingle()
        };
    }
}
