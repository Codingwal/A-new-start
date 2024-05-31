using System.Collections.Generic;
using UnityEngine;

public static class VertexGenerator
{
    public static VertexData GenerateVertexData(Vector2 pos, Vector2[] octaveOffsets, BiomeSettings biomeSettings, float maxPossibleHeight, float terrainScale)
    {
        float height = Noise.GenerateNoise(pos / terrainScale, octaveOffsets, biomeSettings.noiseScale,
                biomeSettings.octaveAmplitudeFactor, biomeSettings.octaveFrequencyFactor, biomeSettings.slopeImpact, maxPossibleHeight) * biomeSettings.heightMultiplier * terrainScale;

        return new(height, 0, new());
    }
}
