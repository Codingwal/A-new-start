using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VertexGenerator
{
    public static float GenerateVertexData(Vector2 pos, int seed, TerrainSettings terrainSettings, float terrainScale, int octaves = 0)
    {
        Vector2[] biomeOctaveOffsets = GenerateOctaveOffsets(seed, 5);
        BiomeSettings biomeSettings = GetBiomeSettings(new Vector2(pos.x, pos.y) / terrainScale, biomeOctaveOffsets, terrainSettings);

        return GenerateVertexData(pos, seed, biomeSettings, terrainScale, octaves);
    }
    public static float GenerateVertexData(Vector2 pos, int seed, BiomeSettings biomeSettings, float terrainScale, int octaves = 0)
    {
        // Generate the map using the biomeSettings
        Vector2[] octaveOffsets;
        if (octaves == 0)
        {
            octaveOffsets = GenerateOctaveOffsets(seed, biomeSettings.octaves);
        }
        else
        {
            octaveOffsets = GenerateOctaveOffsets(seed, octaves);
        }

        // 2 is the max possible height while using octaveAmplitudeFactor = 0.5f (1 + 1/2 + 1/4 + 1/8 + ... approaches 2)
        float height = Noise.GenerateNoise(pos / terrainScale, octaveOffsets, biomeSettings.noiseScale,
        biomeSettings.octaveAmplitudeFactor, biomeSettings.octaveFrequencyFactor, biomeSettings.slopeImpact, 2) * biomeSettings.heightMultiplier * terrainScale;

        height += biomeSettings.heightOffset * terrainScale;

        return height;
    }
    public static Vector2[] GenerateOctaveOffsets(int seed, int octaveCount)
    {
        Vector2[] octaveOffsets = new Vector2[octaveCount];

        System.Random rnd = new(seed);

        for (int i = 0; i < octaveCount; i++)
        {
            // Get random offset for each octave, which will be added to the position, to get different noise depending on the seed

            // Generate the random offsets
            float offsetX = rnd.Next(-100000, 100000);
            float offsetY = rnd.Next(-100000, 100000);

            // Store the offset in an array
            octaveOffsets[i] = new(offsetX, offsetY);
        }
        return octaveOffsets;
    }
    public static BiomeSettings GetBiomeSettings(Vector2 pos, Vector2[] biomeOctaveOffsets, TerrainSettings terrainSettings)
    {
        float biomeValue = Mathf.Clamp01(Noise.GenerateNoise(pos / terrainSettings.terrainScale, biomeOctaveOffsets, terrainSettings.biomeScale, 0.5f, 2, 0, 1.5f));

        // Get the biomes with a value directly above and below the received noise value
        KeyValuePair<float, BiomeSettings> biomeLowerValue = new(float.PositiveInfinity, new());
        KeyValuePair<float, BiomeSettings> biomeHigherValue = new(float.NegativeInfinity, new());
        foreach (KeyValuePair<float, BiomeSettings> biome in terrainSettings.biomes)
        {
            // biomeHeight must be higher, currentLowerBiome height must be lower (greater distance)
            if (biomeValue >= biome.Key && biome.Key < biomeLowerValue.Key)
            {
                biomeLowerValue = biome;
            }

            // biomeHeight must be lower, currentHigherBiome height must be higher (greater distance)   
            if (biomeValue <= biome.Key && biome.Key > biomeHigherValue.Key)
            {
                biomeHigherValue = biome;
            }
        }
        Debug.Assert(biomeLowerValue.Key != float.PositiveInfinity, $"No biome with a lower height found (height = {biomeValue})");
        Debug.Assert(biomeHigherValue.Key != float.NegativeInfinity, $"No biome with a higher height found (height = {biomeValue})");

        // Calculate the biomeSettings of this chunk by lerping between the higher and the lower biomeSetting
        return BiomeSettings.Lerp(biomeLowerValue.Value, biomeHigherValue.Value, Mathf.InverseLerp(biomeLowerValue.Key, biomeHigherValue.Key, biomeValue));
    }
}
