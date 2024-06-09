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

[CreateAssetMenu()]
[Serializable]
public class PlayerSettingsObject : ScriptableObject
{

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
    public SerializableDictonary<float, BiomeSettings> biomes = new();
    public float biomeScale = 5000;
    public float uniformScale = 1;
    public float minHeight = 0;
    public float maxHeight = 150;
    public float minWaterSourceHeight = 0.7f;
    public float terrainScale = 1;
    public float riverWaterGain = 0.01f;
    public int maxRiverCount = 10;
    public int maxRiverGenerationTries = 15;
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
        minHeight = obj.minHeight;
        maxHeight = obj.maxHeight;
        minWaterSourceHeight = obj.minWaterSourceHeight;
        terrainScale = obj.terrainScale;
        riverWaterGain = obj.riverWaterGain;
        maxRiverCount = obj.maxRiverCount;
        maxRiverGenerationTries = obj.maxRiverGenerationTries;
    }
}
[Serializable]
public class BiomeWrapper
{
    public float height;
    public BiomeSettings biomeSettings;
}
[CreateAssetMenu()]
[Serializable]
public class TerrainSettingsObject : ScriptableObject
{
    public List<BiomeWrapper> biomes;
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
