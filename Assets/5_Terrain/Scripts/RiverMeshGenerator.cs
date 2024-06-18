using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEditor;

public static class RiverMeshGenerator
{
    const float tmp = 0.1f;
    const float scalar = 1.5f;
    public static Mesh GenerateRiverMesh(List<List<Vector3>> rivers, int chunkSize)
    {
        // Vector2[] arr = {
        //     new(1, 0),new(1, 0),new(1, 0),new(0, 1),new(0, 1),new(0, 1),new(0, 1),new(0, 1),new(0, 1),new(0, 1),new(0, 10),new(0, 1),new(0, 1),
        //     new(-1, -1),new(-1, -1),new(-1, -1),new(-1, -1),new(-1, -1),new(-1, -1),
        // };

        // rivers.Clear();
        // {
        //     List<Vector3> river = new()
        // {
        //     new(30, 50, 30)
        // };

        //     foreach (Vector2 p in arr)
        //     {
        //         Vector3 op = river[^1];
        //         river.Add(op + new Vector3(p.x, 0, p.y));
        //     }
        //     rivers.Add(river);
        // }

        float halfSize = (chunkSize - 1) / 2f;

        List<Vector3> vertices = new();
        List<int> triangles = new();

        foreach (List<Vector3> river in rivers)
        {
            for (int i = 0; i < river.Count - 1; i++)
            {
                Vector3 point = river[i] - new Vector3(halfSize, 0, halfSize);
                Vector2 direction = new Vector2(river[i + 1].x - river[i].x, river[i + 1].z - river[i].z).normalized;

                // Is actually a Vector2Int
                Vector2 offset = Vector2.Perpendicular(direction);

                Vector2 newPoint = new Vector2(point.x, point.z) + offset * scalar;
                if (i != 0)
                {
                    Vector2 oldPoint = new(vertices[^2].x, vertices[^2].z);

                    float x = (direction.x == 0) ? newPoint.x : ((direction.x > 0) ? Mathf.Max(newPoint.x, oldPoint.x + tmp) : Mathf.Min(newPoint.x, oldPoint.x - tmp));
                    float y = (direction.y == 0) ? newPoint.y : ((direction.y > 0) ? Mathf.Max(newPoint.y, oldPoint.y + tmp) : Mathf.Min(newPoint.y, oldPoint.y - tmp));
                    newPoint = new(x, y);
                }

                vertices.Add(new(newPoint.x, point.y, newPoint.y));

                // -- //

                newPoint = new Vector2(point.x, point.z) - offset * scalar;
                if (i != 0)
                {
                    Vector2 oldPoint = new(vertices[^2].x, vertices[^2].z);

                    float x = (direction.x == 0) ? newPoint.x : ((direction.x > 0) ? Mathf.Max(newPoint.x, oldPoint.x + tmp) : Mathf.Min(newPoint.x, oldPoint.x - tmp));
                    float y = (direction.y == 0) ? newPoint.y : ((direction.y > 0) ? Mathf.Max(newPoint.y, oldPoint.y + tmp) : Mathf.Min(newPoint.y, oldPoint.y - tmp));
                    newPoint = new(x, y);
                }

                vertices.Add(new(newPoint.x, point.y, newPoint.y));

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