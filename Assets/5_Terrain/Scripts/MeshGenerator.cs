using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(VertexData[,] heightMap, int levelOfDetail)
    {
        int meshSize = heightMap.GetLength(0);
        float halfSize = (meshSize - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticesPerLine = ((meshSize - 1) / meshSimplificationIncrement) + 1;

        MeshData meshData = new(verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < meshSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < meshSize; x += meshSimplificationIncrement)
            {
                meshData.vertices[vertexIndex] = new(x - halfSize, heightMap[x, y].height, y - halfSize);

                float slopeX;
                if (x + 1 < meshSize)
                {
                    slopeX = Mathf.Abs(heightMap[x + 1, y].height - heightMap[x, y].height);
                }
                else
                {
                    slopeX = Mathf.Abs(heightMap[x - 1, y].height - heightMap[x, y].height);
                }
                float slopeY;
                if (y + 1 < meshSize)
                {
                    slopeY = Mathf.Abs(heightMap[x, y + 1].height - heightMap[x, y].height);
                }
                else
                {
                    slopeY = Mathf.Abs(heightMap[x, y - 1].height - heightMap[x, y].height);
                }
                float slope = (slopeX + slopeY) / 2;
                meshData.uvs[vertexIndex] = new(slope, 0);

                if (x < meshSize - 1 && y < meshSize - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine, vertexIndex + 1);
                    meshData.AddTriangle(vertexIndex + 1, vertexIndex + verticesPerLine, vertexIndex + verticesPerLine + 1);
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
