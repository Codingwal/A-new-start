using System.Collections.Generic;
using UnityEngine;

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
}