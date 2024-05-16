using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

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
    public void Save<T>(T data, string folder, string fileName)
    {
        FileStream saveFile = File.Create($"{SaveFolder}{folder}/{fileName}.bin");

        formatter.Serialize(saveFile, data);

        saveFile.Close();
    }
    public T Load<T>(string folder, string fileName, bool forceReturn = false)
    {
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

    // Old code

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
}
