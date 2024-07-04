using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine.SocialPlatforms.GameCenter;

public class FileDataHandler
{
    public static readonly string SaveFolder = $"{Application.dataPath}/Data/";

    BinaryFormatter formatter = new();
    SurrogateSelector surrogateSelector = new();

    public FileDataHandler()
    {
        // Create all directories

        // Data directory
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
        }

        // Worlds directory
        if (!Directory.Exists(SaveFolder + "Worlds/"))
        {
            Directory.CreateDirectory(SaveFolder + "Worlds/");
        }

        Vector3SerializationSurrogate v3ss = new();
        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3ss);

        Vector2SerializationSurrogate v2ss = new();
        surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), v2ss);

        formatter.SurrogateSelector = surrogateSelector;

    }
    public List<string> ListAllFilesInDirectory(string folder)
    {
        string[] files = Directory.GetFiles(Path.Combine(SaveFolder, folder));
        List<string> reworkedFileNames = new();
        foreach (string fileName in files)
        {
            if (fileName.Contains(".meta"))
            {
                continue;
            }
            reworkedFileNames.Add(fileName[(Math.Max(fileName.LastIndexOf("/"), fileName.LastIndexOf("\\")) + 1)..(fileName.LastIndexOf("."))]);
        }
        return reworkedFileNames;
    }
    public void Save<T>(T data, string folder, string fileName)
    {
        if (typeof(T) == typeof(WorldData))
        {
            SaveCustom((WorldData)(object)data, folder, fileName);
            return;
        }

        FileStream saveFile = File.Create($"{SaveFolder}{folder}/{fileName}.bin");

        formatter.Serialize(saveFile, data);

        saveFile.Close();
    }
    public T Load<T>(string folder, string fileName, bool forceReturn = false)
    {
        if (typeof(T) == typeof(WorldData))
        {
            return (T)(object)LoadCustom(folder, fileName, forceReturn);
        }

        T data;

        try
        {
            FileStream saveFile = File.Open($"{SaveFolder}{folder}/{fileName}.bin", FileMode.Open);

            try
            {
                data = (T)formatter.Deserialize(saveFile);
            }
            catch (Exception exception)
            {
                if (forceReturn)
                {
                    return default;
                }
                throw exception;
            }
            saveFile.Close();
        }
        catch (Exception exception)
        {
            if (forceReturn)
            {
                return default;
            }
            throw exception;
        }

        return data;
    }
    public void SaveCustom(WorldData data, string folder, string fileName)
    {
        FileStream saveFile = File.Create($"{SaveFolder}{folder}/{fileName}.bin");
        BinaryWriter bw = new(saveFile);

        PlayerData playerData = data.playerData;

        Write(bw, playerData.position);
        Write(bw, playerData.rotation);

        TerrainSettings terrainSettings = data.terrainSettings;
        foreach (Biome biome in terrainSettings.biomes)
        {
            Write(bw, biome.Bounds);

            BiomeSettings biomeSettings = biome.biomeSettings;
            Write(bw, biomeSettings);
        }
        WriteClose(bw);

        bw.Write(terrainSettings.biomeScale);

        bw.Write(terrainSettings.uniformScale);
        bw.Write(terrainSettings.terrainScale);

        bw.Write(terrainSettings.noiseScale);
        bw.Write(terrainSettings.octaves);
        bw.Write(terrainSettings.octaveFrequencyFactor);
        bw.Write(terrainSettings.octaveAmplitudeFactor);

        bw.Write(terrainSettings.generateRivers);
        bw.Write(terrainSettings.minWaterSourceHeight);
        bw.Write(terrainSettings.riverWaterGain);
        bw.Write(terrainSettings.maxRiverCount);
        bw.Write(terrainSettings.maxRiverGenerationTries);
        bw.Write(terrainSettings.minRiverSlope);
        bw.Write(terrainSettings.riverDirectionImpact);

        TerrainData terrainData = data.terrainData;
        bw.Write(terrainData.seed);

        foreach (KeyValuePair<Vector2, ChunkData> pair in terrainData.chunks)
        {
            Write(bw, pair.Key);

            Write(bw, pair.Value.bottomLeft);
            Write(bw, pair.Value.bottomRight);
            Write(bw, pair.Value.topLeft);
            Write(bw, pair.Value.topRight);

            foreach (ListWrapper<VertexData> element in pair.Value.map)
            {
                foreach (VertexData vertexData in element.list)
                {
                    bw.Write(vertexData.height);
                    // bw.Write(vertexData.waterAmount);
                }
                WriteClose(bw);
            }
            WriteClose(bw);
            foreach (ListWrapper<Vector3> river in pair.Value.rivers)
            {
                foreach (Vector3 point in river.list)
                {
                    Write(bw, point);
                    // bw.Write(vertexData.waterAmount);
                }
                WriteClose(bw);
            }
            WriteClose(bw);

            // TODO: Trees
        }
        WriteClose(bw);

        bw.Close();
        saveFile.Close();
    }
    public WorldData LoadCustom(string folder, string fileName, bool forceReturn = false)
    {
        WorldData data = new();

        FileStream saveFile;
        try
        {
            saveFile = File.Open($"{SaveFolder}{folder}/{fileName}.bin", FileMode.Open);
        }
        catch (Exception exception)
        {
            if (forceReturn)
            {
                return default;
            }
            throw exception;
        }

        BinaryReader br = new(saveFile);
        data.playerData.position = Read<Vector3>(br);
        data.playerData.rotation = Read<Vector3>(br);

        float readData = br.ReadSingle();
        while (readData != 2.1059140958881314e+37)
        {
            Biome biome = ScriptableObject.CreateInstance<Biome>();

            biome.Bounds = new Bounds()
            {
                center = new(readData, br.ReadSingle(), br.ReadSingle()),
                extents = Read<Vector3>(br)
            };
            biome.biomeSettings = Read<BiomeSettings>(br);

            data.terrainSettings.biomes.Add(biome);
            readData = br.ReadSingle();
        }

        data.terrainSettings.biomeScale = br.ReadSingle();

        data.terrainSettings.uniformScale = br.ReadSingle();
        data.terrainSettings.terrainScale = br.ReadSingle();

        data.terrainSettings.noiseScale = br.ReadSingle();
        data.terrainSettings.octaves = br.ReadInt32();
        data.terrainSettings.octaveFrequencyFactor = br.ReadSingle();
        data.terrainSettings.octaveAmplitudeFactor = br.ReadSingle();

        data.terrainSettings.generateRivers = br.ReadBoolean();
        data.terrainSettings.minWaterSourceHeight = br.ReadSingle();
        data.terrainSettings.riverWaterGain = br.ReadSingle();
        data.terrainSettings.maxRiverCount = br.ReadInt32();
        data.terrainSettings.maxRiverGenerationTries = br.ReadInt32();
        data.terrainSettings.minRiverSlope = br.ReadSingle();
        data.terrainSettings.riverDirectionImpact = br.ReadSingle();

        data.terrainData.seed = br.ReadInt32();

        readData = br.ReadSingle();

        // chunks dict
        while (readData != 2.1059140958881314e+37)
        {
            SerializableDictonary<Vector2, ChunkData> chunks = new();

            Vector2 key = new()
            {
                x = readData,
                y = br.ReadSingle()
            };

            ChunkData chunkData = new()
            {
                bottomLeft = Read<BiomeSettings>(br),
                bottomRight = Read<BiomeSettings>(br),
                topLeft = Read<BiomeSettings>(br),
                topRight = Read<BiomeSettings>(br),
            };

            readData = br.ReadSingle();

            // heightMapList
            List<ListWrapper<VertexData>> map = new();
            while (readData != 2.1059140958881314e+37)
            {
                List<VertexData> list2 = new();
                // 2nd list
                while (readData != 2.1059140958881314e+37)
                {
                    VertexData vertexData = new()
                    {
                        height = readData,
                        // waterAmount = br.ReadSingle(),
                    };
                    list2.Add(vertexData);
                    readData = br.ReadSingle();
                }
                map.Add(new(list2));
                readData = br.ReadSingle();
            }
            chunkData.map = map;

            List<ListWrapper<Vector3>> rivers = new();
            while (readData != 2.1059140958881314e+37)
            {
                List<Vector3> river = new();
                // 2nd list
                while (readData != 2.1059140958881314e+37)
                {
                    Vector3 point = new()
                    {
                        x = readData,
                        y = br.ReadSingle(),
                        z = br.ReadSingle()
                    };
                    river.Add(point);
                    readData = br.ReadSingle();
                }
                rivers.Add(new(river));
                readData = br.ReadSingle();
            }
            chunkData.rivers = rivers;

            // TODO: Trees
            chunkData.trees = new();

            chunks.TryAdd(key, chunkData);
            readData = br.ReadSingle();
        }

        saveFile.Close();

        return data;
    }
    void WriteClose(BinaryWriter bw)
    {
        bw.Write('}');
        bw.Write('}');
        bw.Write('}');
        bw.Write('}');
    }
    void Write(BinaryWriter bw, Vector3 vec3)
    {
        bw.Write(vec3.x);
        bw.Write(vec3.y);
        bw.Write(vec3.z);
    }
    void Write(BinaryWriter bw, Vector2 vec2)
    {
        bw.Write(vec2.x);
        bw.Write(vec2.y);
    }
    void Write(BinaryWriter bw, Bounds bounds)
    {
        Write(bw, bounds.center);
        Write(bw, bounds.extents);
    }
    void Write(BinaryWriter bw, BiomeSettings biomeSettings)
    {
        bw.Write(biomeSettings.heightMultiplier);
        bw.Write(biomeSettings.slopeImpact);
        bw.Write(biomeSettings.heightOffset);
        bw.Write(biomeSettings.minTreeSpacing);
        // TODO: Trees
    }
    T Read<T>(BinaryReader br)
    {
        if (typeof(T) == typeof(Vector3))
        {
            Vector3 vec3;
            vec3.x = br.ReadSingle();
            vec3.y = br.ReadSingle();
            vec3.z = br.ReadSingle();
            return (T)(object)vec3;
        }
        if (typeof(T) == typeof(Vector2))
        {
            Vector2 vec2;
            vec2.x = br.ReadSingle();
            vec2.y = br.ReadSingle();
            return (T)(object)vec2;
        }
        if (typeof(T) == typeof(BiomeSettings))
        {
            BiomeSettings biomeSettings = new()
            {
                heightMultiplier = br.ReadSingle(),
                slopeImpact = br.ReadSingle(),
                heightOffset = br.ReadSingle(),
                minTreeSpacing = br.ReadSingle(),
                // TODO: Trees
            };
            return (T)(object)biomeSettings;
        }
        throw new();
    }
}


// Old code

// void Write<T>(BinaryWriter bw, T data)
// {
//     if (typeof(T) == typeof(Vector3))
//     {
//         Vector3 vec3 = (Vector3)(object)data;
//         bw.Write(vec3.x);
//         bw.Write(vec3.y);
//         bw.Write(vec3.z);
//         return;
//     }
//     if (typeof(T) == typeof(Vector2))
//     {
//         Vector2 vec2 = (Vector2)(object)data;
//         bw.Write(vec2.x);
//         bw.Write(vec2.y);
//         return;
//     }
// }
// void Write<TKey, TValue>(BinaryWriter bw, SerializableDictonary<TKey, TValue> dict)
// {
//     bw.Write('{');
//     foreach (KeyValuePair<TKey, TValue> pair in dict)
//     {
//         Write(bw, pair.Key);
//         Write(bw, pair.Value);
//     }
//     bw.Write('}');
// }
// void Write<T>(BinaryWriter bw, List<T> list)
// {
//     bw.Write('{');
//     foreach (T element in list)
//     {
//         Debug.LogWarning(typeof(T).ToString());
//         Write(bw, element);
//     }
//     bw.Write('}');
// }
// void Write<T>(BinaryWriter bw, ListWrapper<T> list)
// {
//     Debug.LogWarning("!");
//     Write(bw, list.list);
// }

// public void SaveJSON<T>(T data, string folder, string fileName)
// {
//     folder += "/";

//     string dataString = JsonUtility.ToJson(data, false);

//     File.WriteAllText($"{SaveFolder}{folder}/{fileName}.txt", dataString);
// }
// public T LoadJSON<T>(string folder, string fileName, bool forceReturn = false)
// {   
//     string dataString;
//     try
//     {
//         folder += "/";

//         dataString = File.ReadAllText($"{SaveFolder}{folder}/{fileName}.txt");
//     }
//     catch (Exception exception)
//     {
//         if (forceReturn)
//         {
//             return default;
//         }
//         throw exception;
//     }

//     T obj = JsonUtility.FromJson<T>(dataString);
//     if (obj == null && !forceReturn)
//     {
//         throw new($"Invalid Object in file {SaveFolder}{folder}/{fileName}.txt");
//     }

//     return obj;
// }
