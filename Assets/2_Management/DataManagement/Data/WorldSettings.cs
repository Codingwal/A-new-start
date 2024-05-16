using System;
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
public class TerrainSettings
{
    public float meshHeightMultiplier = 150;
    public float noiseScale = 500;
    public float lacunarity = 2;
    public int octaves = 8;
    public float persistance = 0.5f;
    public float slopeImpact = 1;
    public float uniformScale = 1;
    public float minHeight = 0;
    public float maxHeight = 150;
    public TerrainSettings()
    {

    }
    public TerrainSettings(TerrainSettingsObject obj)
    {
        meshHeightMultiplier = obj.meshHeightMultiplier;
        noiseScale = obj.noiseScale;
        lacunarity = obj.lacunarity;
        octaves = obj.octaves;
        persistance = obj.persistance;
        uniformScale = obj.uniformScale;
        slopeImpact = obj.slopeImpact;
        minHeight = obj.minHeight;
        maxHeight = obj.maxHeight;
    }
}

[CreateAssetMenu()]
[Serializable]
public class TerrainSettingsObject : ScriptableObject
{
    public float meshHeightMultiplier = 150;
    public float noiseScale = 500;
    public float lacunarity = 2;
    public int octaves = 8;
    public float persistance = 0.5f;
    public float slopeImpact = 1;
    public float uniformScale = 1;
    public float minHeight = 0;
    public float maxHeight = 150;
}
