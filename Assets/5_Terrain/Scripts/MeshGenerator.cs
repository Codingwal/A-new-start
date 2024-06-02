using System.Linq;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(VertexData[,] heightMap, int levelOfDetail, int increment)
    {
        int meshSize = heightMap.GetLength(0);
        float halfSize = (meshSize * increment - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticesPerLine = ((meshSize - 1) / (meshSimplificationIncrement / increment)) + 1;

        MeshData meshData = new(verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < meshSize; y += meshSimplificationIncrement / increment)
        {
            for (int x = 0; x < meshSize; x += meshSimplificationIncrement / increment)
            {
                // meshData.vertices[vertexIndex] = new(topLeftX + x, heightMap[x, y].height * heightMultiplier, topLeftZ - y);
                try
                {
                    meshData.vertices[vertexIndex] = new(x * increment - halfSize, heightMap[x, y].height, y * increment - halfSize);
                }
                catch
                {
                    Debug.Log($"1 = {meshSimplificationIncrement / increment}, ms = {meshSize}, vpl = {verticesPerLine}, vpl2 = {(meshSize - 1) / meshSimplificationIncrement * increment}");
                    Debug.Log($"vertices.count = {meshData.vertices.Count()}, vertexIndex = {vertexIndex}, ({x}|{y})");
                }
                // meshData.uvs[vertexIndex] = new(x / (float)meshSize, y / (float)meshSize);  
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
                float slope = (slopeX + slopeY) / 2 / increment;
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
