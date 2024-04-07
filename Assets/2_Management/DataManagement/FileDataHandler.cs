using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine;

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
    }

    public void Save<T>(T data, string folder, string fileName)
    {
        folder += "/";

        string dataString = JsonUtility.ToJson(data, true);

        File.WriteAllText($"{SaveFolder}{folder}/{fileName}.txt", dataString);
    }

    public T Load<T>(string folder, string fileName, bool forceReturn = false)
    {
        string dataString = "";

        try
        {
            folder += "/";

            dataString = File.ReadAllText($"{SaveFolder}{folder}/{fileName}.txt");
        }
        catch (Exception exception)
        {
            if (forceReturn)
            {
                return default;
            }
            throw exception;
        }

        return JsonUtility.FromJson<T>(dataString);
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
}
