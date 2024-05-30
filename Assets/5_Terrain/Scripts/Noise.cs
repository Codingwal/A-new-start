using System;
using UnityEngine;
using UnityEngine.TestTools;

// Sources:
// Procedural terrain generation by Sebastian Lauge:
// https://www.youtube.com/watch?v=wbpMiKiSKm8&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3
// Improvement concept from Josh's Channel
// https://www.youtube.com/watch?v=gsJHzBTPG0Y
public static class Noise
{
    public enum NormalizeMode
    {
        Local,
        Global
    }
    public static float GenerateNoise(Vector2 pos, Vector2[] octaveOffsets, float scale, float persistance, float lacunarity, float slopeImpact, float maxPossibleHeight)
    {
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;
        float slopesSum = 0;

        // For each octave...
        for (int i = 0; i < octaveOffsets.Length; i++)
        {
            // Convert the point on the map [pos.x, pos.y] to the sample point on the perlin noise map [sampleX, sampleY]
            // + octaveOffsets[i].x / + octaveOffsets[i].y to get different noise value for each seed and for each octave in the same seed
            // / scale so that a change in position has a smaller impact on the perlin value -> the noise is stretched by the factor scale
            // * frequency is the reverse effect, but scale also applies to the vertical axis so both are needed
            float sampleX = (pos.x + octaveOffsets[i].x) / scale * frequency;
            float sampleY = (pos.y + octaveOffsets[i].y) / scale * frequency;

            // Get the perlin value for the point and change the range from (0, 1) to (-1, 1)
            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

            float epsilon = 0.01f;

            // Approximate the slope at the point using the derivative and the pythagorean theorem
            float slopeX = (Mathf.PerlinNoise(sampleX + epsilon, sampleY) - perlinValue) / epsilon;
            float slopeY = (Mathf.PerlinNoise(sampleX, sampleY + epsilon) - perlinValue) / epsilon;
            float slope = Mathf.Sqrt(slopeX * slopeX + slopeY * slopeY);

            // Each layer's influence is affected by the slope of this layer and all previous layers
            slopesSum += slope;

            // This result in a value between 0 and 1, a higher slopesSum results in a lower value -> less impact
            float layerInfluence = 1 / (1 + slopesSum * slopeImpact);

            // Add the perlin value times the vertical scale factor (amplitude) to the total height of the point

            if (i != 0)
                perlinValue = perlinValue * 2 - 1;

            noiseHeight += perlinValue * amplitude * layerInfluence;

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        // Calculate the normalized height (range 0 to 1) by using 0 and maxPossibleHeight
        float normalizedHeight = noiseHeight / maxPossibleHeight;

        // Clamp to prevent errors in edge cases
        // return Mathf.Clamp(normalizedHeight, 0, 1);
        return normalizedHeight;
    }
}
