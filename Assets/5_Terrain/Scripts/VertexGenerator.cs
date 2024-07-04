using System.Collections.Generic;
using UnityEngine;

public static class VertexGenerator
{
    public static float GenerateVertexData(Vector2 pos, int seed, TerrainSettings terrainSettings, float terrainScale, int octaves = 0)
    {
        BiomeSettings biomeSettings = GetBiomeSettings(new Vector2(pos.x, pos.y) / terrainScale, terrainSettings, seed);

        return GenerateVertexData(pos, seed, biomeSettings, terrainSettings, terrainScale, octaves);
    }
    public static float GenerateVertexData(Vector2 pos, int seed, BiomeSettings biomeSettings, TerrainSettings terrainSettings, float terrainScale, int octaves = 0)
    {
        // Generate the map using the biomeSettings
        Vector2[] octaveOffsets;
        if (octaves == 0)
        {
            octaveOffsets = GenerateOctaveOffsets(seed, terrainSettings.octaves);
        }
        else
        {
            octaveOffsets = GenerateOctaveOffsets(seed, octaves);
        }

        // 2 is the max possible height while using octaveAmplitudeFactor = 0.5f (1 + 1/2 + 1/4 + 1/8 + ... approaches 2)
        float height = Noise.GenerateNoise(pos / terrainScale, octaveOffsets, terrainSettings.noiseScale,
        terrainSettings.octaveAmplitudeFactor, terrainSettings.octaveFrequencyFactor, biomeSettings.slopeImpact, 2) * biomeSettings.heightMultiplier * terrainScale;

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
    public static BiomeSettings GetBiomeSettings(Vector2 pos, TerrainSettings terrainSettings, int seed)
    {
        Vector2[] biomeOctaveOffsets = GenerateOctaveOffsets(seed + 10, 1);
        float height = Noise.GenerateNoise(pos / terrainSettings.terrainScale, biomeOctaveOffsets, terrainSettings.biomeScale, 0.5f, 2, 0, 1);

        biomeOctaveOffsets = GenerateOctaveOffsets(seed + 20, 1);
        float temperature = Noise.GenerateNoise(pos / terrainSettings.terrainScale, biomeOctaveOffsets, terrainSettings.biomeScale, 0.5f, 2, 0, 1);

        biomeOctaveOffsets = GenerateOctaveOffsets(seed + 30, 1);
        float humidity = Noise.GenerateNoise(pos / terrainSettings.terrainScale, biomeOctaveOffsets, terrainSettings.biomeScale, 0.5f, 2, 0, 1);

        // Get the biomes with a value directly above and below the received noise value
        KeyValuePair<float, BiomeSettings> closestBiome = new(float.PositiveInfinity, null);
        KeyValuePair<float, BiomeSettings> secondClosestBiome = new(float.PositiveInfinity, null);
        foreach (Biome biome in terrainSettings.biomes)
        {
            float sqrDistance = biome.Bounds.SqrDistance(new(height, temperature, humidity));

            if (sqrDistance < closestBiome.Key)
            {
                secondClosestBiome = closestBiome;
                closestBiome = new(sqrDistance, biome.biomeSettings);
            }
            else if (sqrDistance < secondClosestBiome.Key)
            {
                secondClosestBiome = new(sqrDistance, biome.biomeSettings);
            }
        }

        Debug.Assert(closestBiome.Value != null, $"Couldn't find closestBiome! There are {terrainSettings.biomes.Count} biomes");
        Debug.Assert(secondClosestBiome.Value != null, $"Couldn't find secondClosestBiome! There are {terrainSettings.biomes.Count} biomes");

        // Minimum of 0.01f to prevent divisionByZero errors if the point lies inside a biome bound (distance calculation returns 0 in this case)
        float t = closestBiome.Key / Mathf.Max(closestBiome.Key + secondClosestBiome.Key, 0.01f);

        return BiomeSettings.Lerp(closestBiome.Value, secondClosestBiome.Value, t);
    }
}
