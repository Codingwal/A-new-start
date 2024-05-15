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
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, float slopeImpact, Vector2 offset)
    {
        // This will be returned
        float[,] noiseMap = new float[mapWidth, mapHeight];

        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;

        {
            System.Random prng = new(seed);
            float amplitude = 1;

            // For each octave...
            for (int i = 0; i < octaves; i++)
            {
                // Get random offset for each octave, which will be added to the position, to get different noise depending on the seed

                // Generate the random offsets and add the offset of the chunk to that value
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) - offset.y;

                // Store the offset in an array
                octaveOffsets[i] = new(offsetX, offsetY);


                maxPossibleHeight += amplitude;
                amplitude *= persistance;
            }
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // For each point of the chunk...
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                float slopesSum = 0;

                // For each octave at that point...
                for (int i = 0; i < octaves; i++)
                {
                    // Convert the point on the map [x, y] to the sample point on the perlin noise map [sampleX, sampleY]
                    // -halfWidth / -halfHeight because the middle of the chunk actually lies at [0, 0] or the chunk center in general
                    // +octaveOffsets[i].x / +octaveOffsets[i].y to get different noise map for each seed and for each octave in the same seed
                    // /scale so that a change in position has a smaller impact on the perlin value -> the noise is stretched by the factor scale
                    // *frequency is the reverse effect, but scale also applies to the vertical axis so both are needed
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

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

                    // TODO: Change this?
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // Store the calculated height in the 2d height array
                noiseMap[x, y] = noiseHeight;
            }
        }

        // For each point...
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Calculate the normalized height (range 0 to 1) by using 0 and maxPossibleHeight
                // +1 / 2 to reverse the operation done to change the range from (0, 1) to (-1, 1)
                // /maxPossibleHeight to normalize
                // *1.75 because realistically, maxPossibleHeight will never be reached
                float normalizedHeight = noiseMap[x, y] / (maxPossibleHeight / 1.75f);
                noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, 1);

            }
        }

        return noiseMap;
    }
}
