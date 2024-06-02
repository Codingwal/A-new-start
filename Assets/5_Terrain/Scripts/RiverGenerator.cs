using System.Collections.Generic;
using UnityEngine;

public static class RiverGenerator
{
    public static List<River> GenerateRivers(Vector2 center, int sectorSize, int seed, TerrainSettings terrainSettings)
    {
        List<River> rivers = new();

        System.Random rnd = new(seed);

        

        return rivers;
    }

    public class River
    {
        public List<Vector2> points = new();
    }
}
