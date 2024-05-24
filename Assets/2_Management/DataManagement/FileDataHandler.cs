using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Unity.IO.LowLevel.Unsafe;

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
        bw.Write(terrainSettings.meshHeightMultiplier);
        bw.Write(terrainSettings.noiseScale);
        bw.Write(terrainSettings.lacunarity);
        bw.Write(terrainSettings.octaves);
        bw.Write(terrainSettings.persistance);
        bw.Write(terrainSettings.slopeImpact);
        bw.Write(terrainSettings.uniformScale);
        bw.Write(terrainSettings.minHeight);
        bw.Write(terrainSettings.maxHeight);
        bw.Write(terrainSettings.minWaterSourceHeight);

        TerrainData terrainData = data.terrainData;
        bw.Write(terrainData.seed);

        foreach (KeyValuePair<Vector2, ChunkData> pair in terrainData.chunks)
        {
            Write(bw, pair.Key);
            foreach (ListWrapper<float> element in pair.Value.heightMap)
            {
                foreach (float value in element.list)
                {
                    bw.Write(value);
                }
                WriteClose(bw);
            }
            WriteClose(bw);
            foreach (ListWrapper<VertexWaterInfo> element in pair.Value.riverMap)
            {
                foreach (VertexWaterInfo info in element.list)
                {
                    bw.Write(info.amount);
                    Write(bw, info.velocity);
                }
                WriteClose(bw);
            }
            WriteClose(bw);
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

        data.terrainSettings.meshHeightMultiplier = br.ReadSingle();
        data.terrainSettings.noiseScale = br.ReadSingle();
        data.terrainSettings.lacunarity = br.ReadSingle();
        data.terrainSettings.octaves = br.ReadInt32();
        data.terrainSettings.persistance = br.ReadSingle();
        data.terrainSettings.slopeImpact = br.ReadSingle();
        data.terrainSettings.uniformScale = br.ReadSingle();
        data.terrainSettings.minHeight = br.ReadSingle();
        data.terrainSettings.maxHeight = br.ReadSingle();
        data.terrainSettings.minWaterSourceHeight = br.ReadSingle();

        data.terrainData.seed = br.ReadInt32();

        float readData = br.ReadSingle();

        // chunks dict
        while (readData != 2.1059140958881314e+37)
        {
            SerializableDictonary<Vector2, ChunkData> chunks = new();

            Vector2 key;
            key.x = readData;
            readData = br.ReadSingle();
            key.y = readData;
            readData = br.ReadSingle();

            // heightMapList
            List<ListWrapper<float>> heightMapList = new();
            while (readData != 2.1059140958881314e+37)
            {
                List<float> list2 = new();
                // 2nd list
                while (readData != 2.1059140958881314e+37)
                {
                    list2.Add(readData);
                    readData = br.ReadSingle();
                }
                heightMapList.Add(new(list2));
                readData = br.ReadSingle();
            }
            // heightMapList
            List<ListWrapper<VertexWaterInfo>> riverMapList = new();
            while (readData != 2.1059140958881314e+37)
            {
                List<VertexWaterInfo> list2 = new();
                // 2nd list
                while (readData != 2.1059140958881314e+37)
                {
                    VertexWaterInfo info = new()
                    {
                        amount = readData,
                        velocity = Read<Vector2>(br)
                    };
                    list2.Add(info);
                    readData = br.ReadSingle();
                }
                riverMapList.Add(new(list2));
                readData = br.ReadSingle();
            }
            chunks.TryAdd(key, new(heightMapList, riverMapList));
            readData = br.ReadSingle();
        }

        saveFile.Close();

        return data;
    }
    void WriteOpen(BinaryWriter bw)
    {
        bw.Write('{');
        bw.Write('{');
        bw.Write('{');
        bw.Write('{');
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
