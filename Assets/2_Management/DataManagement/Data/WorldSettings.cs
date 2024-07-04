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
    // Terrain
    public float heightMultiplier;
    public float slopeImpact;
    public float heightOffset;

    [Header("Trees")]
    public float minTreeSpacing;
    public SerializableDictonary<float, TreeType> trees;
    public static BiomeSettings Lerp(BiomeSettings a, BiomeSettings b, float t)
    {
        return new()
        {
            heightMultiplier = Mathf.Lerp(a.heightMultiplier, b.heightMultiplier, t),
            slopeImpact = Mathf.Lerp(a.slopeImpact, b.slopeImpact, t),
            heightOffset = Mathf.Lerp(a.heightOffset, b.heightOffset, t),
            minTreeSpacing = Mathf.Lerp(a.minTreeSpacing, b.minTreeSpacing, t),
        };

    }
}
[Serializable]
public class TerrainSettings
{
    // Biomes
    public List<Biome> biomes = new();
    public float biomeScale;

    // Scale
    public float uniformScale;
    public float terrainScale;

    // Noise settings
    public float noiseScale;
    public int octaves;
    public float octaveFrequencyFactor;
    public float octaveAmplitudeFactor;

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
        biomes = obj.biomes;
        biomeScale = obj.biomeScale;
        uniformScale = obj.uniformScale;
        terrainScale = obj.terrainScale;

        noiseScale = obj.noiseScale;
        octaves = obj.octaves;
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
