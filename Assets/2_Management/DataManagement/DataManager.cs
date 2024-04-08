using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DataManager
{
    public static string currentWorldName;
    public static string fileName;

    private static FileDataHandler dataHandler = new();

    private static List<IDataCallbackReceiver> dataPersistanceObjects;

    [Space]
    [Header("Default Worldsettings meshHeightCurve")]
    public static AnimationCurve meshHeightCurve;
    public static void NewWorld(string worldName)
    {
        // Create a new WorldData
        WorldData worldData = WorldData.NewWorld("Default");

        // Set the current world to the new world
        currentWorldName = worldName;

        // Save the WorldData
        dataHandler.Save(worldData, "Worlds", worldName);
    }
    public static Dictionary<string, WorldData> GetAllWorlds()
    {
        Dictionary<string, WorldData> worlds = new();
        List<string> worldNames = dataHandler.ListAllFilesInDirectory("Worlds");
        foreach (string worldName in worldNames)
        {
            worlds[worldName] = dataHandler.Load<WorldData>("Worlds", worldName);
        }
        return worlds;
    }
    public static WorldSettings GetWorldSettings()
    {
        return WorldSettings.Default;
    }
    public static void LoadWorld()
    {
        dataPersistanceObjects = FindAllDataPersistanceObjects();

        if (currentWorldName == "")
        {
            Debug.Log("Canceled LoadWorld: CurrentWorldName is empty");
            return;
        }
        // Load the WorldData
        WorldData worldData = dataHandler.Load<WorldData>("Worlds", currentWorldName);

        // Let each IDataPersistance object load the WorldData
        foreach (IDataCallbackReceiver dataPersistanceObject in dataPersistanceObjects)
        {
            dataPersistanceObject.LoadData(worldData);
        }
    }

    public static void SaveWorld()
    {
        Debug.Log("Saving world");
        
        dataPersistanceObjects = FindAllDataPersistanceObjects();

        // Load the WorldData
        WorldData worldData = dataHandler.Load<WorldData>("Worlds", currentWorldName);

        // Let each IDataPersistance object change the worldData
        foreach (IDataCallbackReceiver dataPersistanceObject in dataPersistanceObjects)
        {
            dataPersistanceObject.SaveData(worldData);
        }

        // Save the WorldData
        dataHandler.Save(worldData, "Worlds", currentWorldName);
    }

    private static List<IDataCallbackReceiver> FindAllDataPersistanceObjects()
    {
        IEnumerable<IDataCallbackReceiver> dataPersistanceObjects = Object.FindObjectsOfType<MonoBehaviour>().OfType<IDataCallbackReceiver>();

        return new List<IDataCallbackReceiver>(dataPersistanceObjects);
    }
}
