using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
[Serializable]
public class TerrainSettingsObject : ScriptableObject
{
    [Header("Biomes")]
    public List<BiomeWrapper> biomes = new() { new() };
    public float biomeScale = 5000;

    [Header("Scale")]
    public float uniformScale = 1;
    public float terrainScale = 1;

    [Header("Unused??")]
    public float minHeight = 0;
    public float maxHeight = 150;

    [Header("Rivers")]
    public bool generateRivers = true;
    public float minWaterSourceHeight = 0.7f;
    public float riverWaterGain = 5;
    public int maxRiverCount = 10;
    public int maxRiverGenerationTries = 15;
    public float minRiverSlope = 0.0001f;
    public float riverDirectionImpact = 0;
}