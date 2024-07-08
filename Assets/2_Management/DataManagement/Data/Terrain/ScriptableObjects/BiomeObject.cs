using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Terrain/BiomeObject")]
public class BiomeObject : ScriptableObject
{
    public new Biomes name; // new because it hides a base member
    [Header("[Height, Temperature, Humidity]")]

    [SerializeField] Vector3 min;
    [SerializeField] Vector3 max;
    public BiomeSettingsObject biomeSettings;

    public static explicit operator Biome(BiomeObject obj)
    {
        Bounds bounds = new();
        bounds.SetMinMax(obj.min, obj.max);

        return new()
        {
            name = obj.name,
            bounds = bounds,
            biomeSettings = (BiomeSettings)obj.biomeSettings
        };
    }
    public static List<Biome> ToBiomeList(List<BiomeObject> objList)
    {
        List<Biome> list = new();

        foreach (BiomeObject biomeObj in objList)
        {
            list.Add((Biome)biomeObj);
        }
        return list;
    }
}
[Serializable]
public class BiomeSettingsObject
{
    // Terrain
    public float heightMultiplier;
    public float slopeImpact;
    public float heightOffset;

    [Header("Trees")]
    public float minTreeSpacing;
    public List<BiomeTreeTypeObject> trees = new();
    public static explicit operator BiomeSettings(BiomeSettingsObject obj)
    {
        return new()
        {
            heightMultiplier = obj.heightMultiplier,
            slopeImpact = obj.slopeImpact,
            heightOffset = obj.heightOffset,
            minTreeSpacing = obj.minTreeSpacing,
            trees = BiomeTreeTypeObject.ToBiomeTreeTypeList(obj.trees)
        };
    }
}
[Serializable]
public class BiomeTreeTypeObject
{
    public float chance;
    public TreeTypeObject treeType;

    public static explicit operator BiomeTreeType(BiomeTreeTypeObject obj)
    {
        return new()
        {
            chance = obj.chance,
            treeType = (TreeType)obj.treeType
        };
    }
    public static List<BiomeTreeType> ToBiomeTreeTypeList(List<BiomeTreeTypeObject> obj)
    {
        List<BiomeTreeType> list = new();
        foreach (BiomeTreeTypeObject element in obj)
        {
            list.Add((BiomeTreeType)element);
        }
        return list;
    }
}