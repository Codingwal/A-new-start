using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
[Serializable]
public class TerrainSettingsObject : ScriptableObject
{
    [Header("Biomes")]
    public List<Biome> biomes;
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
}