using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldData
{
    public PlayerSettings playerSettings;
    public PlayerData playerData;
    public TerrainSettings terrainSettings;
    public TerrainData terrainData;

    public static WorldData NewWorld(TerrainSettings terrainSettings, PlayerSettings playerSettings, int seed)
    {

        return new()
        {
            terrainSettings = terrainSettings,
            playerSettings = playerSettings,
            terrainData = TerrainData.Default(seed),
            playerData = PlayerData.Default
        };
    }
    public WorldData()
    {
        terrainSettings = new();
        playerSettings = new();
        terrainData = TerrainData.Default(0);
        playerData = PlayerData.Default;
    }
}