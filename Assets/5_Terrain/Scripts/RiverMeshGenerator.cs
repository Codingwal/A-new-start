using System.Collections.Generic;
using UnityEngine;

public static class RiverMeshGenerator
{
    public static Mesh GenerateRiverMesh(List<List<Vector3>> rivers)
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();

        foreach (List<Vector3> river in rivers)
        {
            for (int i = 0; i < river.Count - 1; i++)
            {
                Vector3 point = river[i];
                Vector2Int direction = Vector2Int.RoundToInt(new(river[i + 1].x - point.x, river[i + 1].y - point.y));

                // Is actually a Vector2Int
                Vector2 offset = Vector2.Perpendicular(direction);

                vertices.Add(new(point.x + offset.x, point.y, point.z + offset.y));
                vertices.Add(new(point.x - offset.x, point.y, point.z - offset.y));

                if (i == river.Count - 2) continue;

                int vertexIndex = i * 2;
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 1);
            }
        }


        return new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
    }
}
