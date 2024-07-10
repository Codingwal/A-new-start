using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;
    public MeshData(int meshSize)
    {
        vertices = new Vector3[meshSize * meshSize];
        uvs = new Vector2[meshSize * meshSize];
        triangles = new int[(meshSize - 1) * (meshSize - 1) * 6];
    }
    public MeshData(Mesh mesh)
    {
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        uvs = mesh.uv;
    }
    public MeshData(Vector3[] vertices, int[] triangles, Vector2[] uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
    }
    public void AddTriangle(int a, int b, int c)
    {
        if (triangleIndex + 2 > triangles.Length)
        {
            return;
        }
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;

    }
    public Mesh CreateMesh()
    {
        Mesh mesh = new()
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();
        return mesh;
    }
    public static MeshData CombineMeshes(MeshData mesh1, MeshData mesh2)
    {
        List<Vector3> vertices = mesh1.vertices.ToList();
        List<int> triangles = mesh1.triangles.ToList();
        List<Vector2> uvs = mesh1.uvs.ToList();

        foreach (int i in mesh2.triangles)
        {
            int indexInVertices = vertices.FindIndex(x => mesh2.vertices[i] == x);

            if (indexInVertices == -1) // If the vertex wasn't found
            {
                // Add the vertex to the vertices list (and also add the corresponding uv to the uvs list)
                vertices.Add(mesh2.vertices[i]);
                uvs.Add(mesh2.uvs[i]);

                // Add the new index to the triangles list
                triangles.Add(vertices.Count - 1);
            }
            else
            {
                // Add the index of the vertex to the triangles list
                triangles.Add(indexInVertices);
            }
        }
        return new(vertices.ToArray(), triangles.ToArray(), uvs.ToArray());
    }
}
