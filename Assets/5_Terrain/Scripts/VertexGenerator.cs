using System.Collections.Generic;
using UnityEngine;

public static class VertexGenerator
{
    public static VertexData GenerateVertexData(Vector2 pos, Vector2[] octaveOffsets, BiomeSettings biomeSettings, float maxPossibleHeight)
    {
        float height = Noise.GenerateNoise(pos, octaveOffsets, biomeSettings.noiseScale,
                biomeSettings.octaveAmplitudeFactor, biomeSettings.octaveFrequencyFactor, biomeSettings.slopeImpact, maxPossibleHeight) * biomeSettings.heightMultiplier;

        return new(height, 0, new());
    }
}
