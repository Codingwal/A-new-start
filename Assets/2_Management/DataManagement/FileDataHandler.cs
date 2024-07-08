using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Runtime.Serialization;
using System.Xml.Serialization;

public class FileDataHandler
{
    public static readonly string SaveFolder = $"{Application.dataPath}/Data/";

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

        SurrogateSelector surrogateSelector = new();

        Vector3SerializationSurrogate vector3Surrogate = new();
        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Surrogate);
        Vector2SerializationSurrogate vector2Surrogate = new();
        surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2Surrogate);
        BoundsSerializationSurrogate boundsSurrogate = new();
        surrogateSelector.AddSurrogate(typeof(Bounds), new StreamingContext(StreamingContextStates.All), boundsSurrogate);

        // formatter.SurrogateSelector = surrogateSelector;
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
        FileStream saveFile = File.Create($"{SaveFolder}{folder}/{fileName}.bin");

        if (typeof(T) == typeof(WorldData))
            Serializer.Serialize(saveFile, (WorldData)(object)data);
        else
            Debug.LogError($"ERROR SAVING DATA: Can't save object of type {typeof(T)}");

        saveFile.Close();
    }
    public T Load<T>(string folder, string fileName, bool forceReturn = false)
    {
        T data = default;

        try
        {
            FileStream saveFile = File.Open($"{SaveFolder}{folder}/{fileName}.bin", FileMode.Open);

            try
            {
                if (typeof(T) == typeof(WorldData))
                    data = (T)(object)Deserializer.Deserialize(saveFile);
                else
                    Debug.LogError($"ERROR LOADING DATA: Can't load object of type {typeof(T)}");
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
}