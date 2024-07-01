using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

[Serializable]
public class PlayerSettings
{
    public PlayerSettings()
    {

    }
    public PlayerSettings(PlayerSettingsObject obj)
    {

    }
}

[Serializable]
public class BiomeSettings
{
    public float heightMultiplier = 150;
    public int octaves = 8;
    public float noiseScale = 500;
    public float octaveFrequencyFactor = 2;
    public float octaveAmplitudeFactor = 0.5f;
    public float slopeImpact = 1;
    public float heightOffset = 0;
    public static BiomeSettings Lerp(BiomeSettings a, BiomeSettings b, float t)
    {
        return new()
        {
            heightMultiplier = Mathf.Lerp(a.heightMultiplier, b.heightMultiplier, t),
            octaves = Mathf.RoundToInt(Mathf.Lerp(a.octaves, b.octaves, t)),
            noiseScale = Mathf.Lerp(a.noiseScale, b.noiseScale, t),
            octaveFrequencyFactor = Mathf.Lerp(a.octaveFrequencyFactor, b.octaveFrequencyFactor, t),
            octaveAmplitudeFactor = Mathf.Lerp(a.octaveAmplitudeFactor, b.octaveAmplitudeFactor, t),
            slopeImpact = Mathf.Lerp(a.slopeImpact, b.slopeImpact, t),
            heightOffset = Mathf.Lerp(a.heightOffset, b.heightOffset, t),
        };

    }
}
[Serializable]
public class TerrainSettings
{
    // Biomes
    public SerializableDictonary<float, BiomeSettings> biomes = new();
    public float biomeScale = 5000;

    // Scale
    public float uniformScale = 1;
    public float terrainScale = 1;

    // Unused??
    public float minHeight = 0;
    public float maxHeight = 150;

    // Rivers
    public bool generateRivers;
    public float minWaterSourceHeight = 0.7f;
    public float riverWaterGain = 0.01f;
    public int maxRiverCount = 10;
    public int maxRiverGenerationTries = 15;
    public float minRiverSlope = 0.0001f;
    public float riverDirectionImpact = 0;
    public TerrainSettings()
    {

    }
    public TerrainSettings(TerrainSettingsObject obj)
    {
        foreach (BiomeWrapper biome in obj.biomes)
        {
            biomes[biome.height] = biome.biomeSettings;
        }
        biomeScale = obj.biomeScale;
        uniformScale = obj.uniformScale;
        terrainScale = obj.terrainScale;

        minHeight = obj.minHeight;
        maxHeight = obj.maxHeight;

        generateRivers = obj.generateRivers;
        minWaterSourceHeight = obj.minWaterSourceHeight;
        riverWaterGain = obj.riverWaterGain;
        maxRiverCount = obj.maxRiverCount;
        maxRiverGenerationTries = obj.maxRiverGenerationTries;
        minRiverSlope = obj.minRiverSlope;
        riverDirectionImpact = obj.riverDirectionImpact;
    }
}
[Serializable]
public class BiomeWrapper
{
    [Range(0, 1)]
    public float height = 0;
    public BiomeSettings biomeSettings = new();
}
