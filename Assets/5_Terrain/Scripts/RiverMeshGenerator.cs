using System.Collections.Generic;
using UnityEngine;

public static class RiverMeshGenerator
{
    public static Mesh GenerateRiverMesh(List<List<Vector3>> rivers, int chunkSize)
    {
        float halfSize = (chunkSize - 1) / 2f;

        List<Vector3> vertices = new();
        List<int> triangles = new();

        foreach (List<Vector3> river in rivers)
        {
            for (int i = 0; i < river.Count - 1; i++)
            {
                Vector3 point = river[i] - new Vector3(halfSize, 0, halfSize);
                Vector2Int direction = Vector2Int.RoundToInt(new(river[i + 1].x - river[i].x, river[i + 1].y - river[i].y));

                // Is actually a Vector2Int
                Vector2 offset = Vector2.Perpendicular(direction);

                // Vector2 newPoint = new(Mathf.Max(Mathf.Abs(point.x + offset.x), point.z + offset.y);


                // // Project 
                // if (i != 0)
                // {
                //     Vector2 normalizedDirection = ((Vector2)direction).normalized;
                //     float newPointProjection = Vector2.Dot(newPoint, normalizedDirection);
                //     float lastPointProjection = Vector2.Dot(new(vertices[^2].x, vertices[^2].y), normalizedDirection);

                //     if (newPointProjection < lastPointProjection)
                //         Debug.Log($": p = {point}, np = {newPoint}, op = {new Vector2(vertices[^2].x, vertices[^2].y)}, d = {normalizedDirection}");
                // }

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