using System.Collections.Generic;
using UnityEngine;

public class Biome
{
    public Biomes name; // new because it hides a base member
    public Bounds bounds;
    public BiomeSettings biomeSettings;
}
public class BiomeSettings
{
    // Terrain
    public float heightMultiplier;
    public float slopeImpact;
    public float heightOffset;

    // Trees
    public float minTreeSpacing;
    public List<BiomeTreeType> trees = new();

    /// <summary>
    /// Lerps all biomeSettings.
    /// </summary>
    public static BiomeSettings Lerp(BiomeSettings a, BiomeSettings b, float t)
    {
        return new()
        {
            heightMultiplier = Mathf.Lerp(a.heightMultiplier, b.heightMultiplier, t),
            slopeImpact = Mathf.Lerp(a.slopeImpact, b.slopeImpact, t),
            heightOffset = Mathf.Lerp(a.heightOffset, b.heightOffset, t),
            minTreeSpacing = Mathf.Lerp(a.minTreeSpacing, b.minTreeSpacing, t),
            trees = LerpTrees(a.trees, b.trees, t)
        };
    }

    /// <summary>
    /// <para>Lerps all settings relevant for terrain generation.</para>
    /// All other settings are set to default and should not be used!
    /// </summary>
    public static BiomeSettings LerpTerrain(BiomeSettings a, BiomeSettings b, float t)
    {
        return new()
        {
            heightMultiplier = Mathf.Lerp(a.heightMultiplier, b.heightMultiplier, t),
            slopeImpact = Mathf.Lerp(a.slopeImpact, b.slopeImpact, t),
            heightOffset = Mathf.Lerp(a.heightOffset, b.heightOffset, t)
        };
    }
    /// <summary>
    /// Lerps between all trees
    /// </summary>
    /// <returns>A list containing all treeTypes and their chance so that the total chance is equal to one</returns>
    public static List<BiomeTreeType> LerpTrees(List<BiomeTreeType> a, List<BiomeTreeType> b, float t)
    {
        List<BiomeTreeType> trees = new();

        foreach (BiomeTreeType type in a)
        {
            trees.Add(new(type.chance * (1 - t), type.treeType));
        }
        foreach (BiomeTreeType type in b)
        {
            trees.Add(new(type.chance * t, type.treeType));
        }
        return trees;
    }
}
public enum Biomes
{
    DeepOcean,
    Ocean,
    Beach,
    Plains,
    Forest,
    LowMountains,
    SnowyMountains
}