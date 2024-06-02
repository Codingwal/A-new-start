using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VertexGenerator
{
    public static float GenerateVertexData(Vector2 pos, int seed, BiomeSettings biomeSettings, int increment, float terrainScale)
    {
        // Generate the map using the biomeSettings
        Vector2[] octaveOffsets = GenerateOctaveOffsets(seed, biomeSettings.octaves);

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
}
