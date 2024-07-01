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
    public float slopeImpact = 1;
    public float heightOffset = 0;
    public static BiomeSettings Lerp(BiomeSettings a, BiomeSettings b, float t)
    {
        return new()
        {
            heightMultiplier = Mathf.Lerp(a.heightMultiplier, b.heightMultiplier, t),
            octaves = Mathf.RoundToInt(Mathf.Lerp(a.octaves, b.octaves, t)),
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
    public float biomeScale;

    // Scale
    public float uniformScale;
    public float terrainScale;

    // Noise settings
    public float noiseScale;
    public float octaveFrequencyFactor;
    public float octaveAmplitudeFactor;

    // Unused??
    public float minHeight;
    public float maxHeight;

    // Rivers
    public bool generateRivers;
    public float minWaterSourceHeight;
    public float riverWaterGain;
    public int maxRiverCount;
    public int maxRiverGenerationTries;
    public float minRiverSlope;
    public float riverDirectionImpact;
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

        noiseScale = obj.noiseScale;
        octaveFrequencyFactor = obj.octaveFrequencyFactor;
        octaveAmplitudeFactor = obj.octaveAmplitudeFactor;

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
