using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, int levelOfDetail)
    {
        int meshSize = heightMap.GetLength(0);
        float topLeftX = (meshSize - 1) / -2f;
        float topLeftZ = (meshSize - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticesPerLine = ((meshSize - 1) / meshSimplificationIncrement) + 1;

        MeshData meshData = new(verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < meshSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < meshSize; x += meshSimplificationIncrement)
            {
                // meshData.vertices[vertexIndex] = new(topLeftX + x, heightMap[x, y] * heightMultiplier, topLeftZ - y);
                meshData.vertices[vertexIndex] = new(topLeftX + x, heightMap[x, y], topLeftZ - y);
                meshData.uvs[vertexIndex] = new(x / (float)meshSize, y / (float)meshSize);

                if (x < meshSize - 1 && y < meshSize - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }
        return meshData;
    }
}

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
}
