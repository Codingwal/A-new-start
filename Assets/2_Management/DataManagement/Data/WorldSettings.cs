using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSettings
{
    public PlayerSettings playerSettings;
    public TerrainSettings terrainSettings;

    public static WorldSettings Default
    {
        get
        {
            return new()
            {
                playerSettings = PlayerSettings.Default,
                terrainSettings = TerrainSettings.Default
            };
        }
    }
}
public class PlayerSettings
{
    public static PlayerSettings Default
    {
        get
        {
            return new();
        }
    }
}
public class TerrainSettings
{
    public AnimationCurve meshHeightCurve;
    public float meshHeightMultiplier;
    public float noiseScale;
    public float lacunarity;
    public int octaves;
    public float persistance;
    public float uniformScale;

    public float MinHeight
    {
        get
        {
            return 0;
        }
    }
    public float MaxHeight
    {
        get
        {
            return 150;
        }
    }
    public static TerrainSettings Default
    {
        get
        {
            return new()
            {
                meshHeightCurve = DataManager.meshHeightCurve,
                meshHeightMultiplier = 150,
                noiseScale = 500,
                octaves = 8,
                lacunarity = 2f,
                persistance = 0.5f,
                uniformScale = 1f
            };
        }
    }
}
