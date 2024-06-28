using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
[Serializable]
public class TerrainSettingsObject : ScriptableObject
{
    public List<BiomeWrapper> biomes = new() { new() };
    public float biomeScale = 5000;
    public float uniformScale = 1;
    public float minHeight = 0;
    public float maxHeight = 150;
    public float minWaterSourceHeight = 0.7f;
    public float terrainScale = 1;
    public float riverWaterGain = 5;
    public int maxRiverCount = 10;
    public int maxRiverGenerationTries = 15;
}