using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/TerrainSettingsObject")]
[Serializable]
public class TerrainSettingsObject : ScriptableObject
{
    [Header("Biomes")]
    public List<BiomeObject> biomes;
    public float biomeScale;

    [Header("Scale")]
    public float uniformScale;
    public float terrainScale;

    [Header("Noise settings")]
    public float noiseScale;
    public int octaves;
    public float octaveFrequencyFactor;
    public float octaveAmplitudeFactor;

    [Header("Rivers")]
    public bool generateRivers;
    public float minWaterSourceHeight;
    public float riverWaterGain;
    public int maxRiverCount;
    public int maxRiverGenerationTries;
    public float minRiverSlope;
    public float riverDirectionImpact;

    public static explicit operator TerrainSettings(TerrainSettingsObject obj)
    {
        return new()
        {
            biomes = BiomeObject.ToBiomeList(obj.biomes),
            biomeScale = obj.biomeScale,
            uniformScale = obj.uniformScale,
            terrainScale = obj.terrainScale,

            noiseScale = obj.noiseScale,
            octaves = obj.octaves,
            octaveFrequencyFactor = obj.octaveFrequencyFactor,
            octaveAmplitudeFactor = obj.octaveAmplitudeFactor,

            generateRivers = obj.generateRivers,
            minWaterSourceHeight = obj.minWaterSourceHeight,
            riverWaterGain = obj.riverWaterGain,
            maxRiverCount = obj.maxRiverCount,
            maxRiverGenerationTries = obj.maxRiverGenerationTries,
            minRiverSlope = obj.minRiverSlope,
            riverDirectionImpact = obj.riverDirectionImpact
        };
    }
}