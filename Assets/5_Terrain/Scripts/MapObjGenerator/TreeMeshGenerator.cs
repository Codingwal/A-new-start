using System;
using System.Collections.Generic;
using UnityEngine;

public static class TreeMeshGenerator
{
    public static Mesh CreateTreeMesh(ChunkData map, int lod, TreeMeshes treeMeshes)
    {
        int halfChunkSize = MapGenerator.Instance.chunkSize / 2;

        CombineInstance[] combine = new CombineInstance[map.trees.Count];
        for (int i = 0; i < map.trees.Count; i++)
        {
            TreeData tree = map.trees[i];

            combine[i].mesh = treeMeshes.GetMesh(tree.type, lod);

            Vector3 localPosition = new(tree.pos.x, map.map[Mathf.RoundToInt(tree.pos.x + halfChunkSize), Mathf.RoundToInt(tree.pos.y + halfChunkSize)].height - 0.3f, tree.pos.y);

            // Gives the object a random but deterministic rotation
            System.Random rnd = new((int)tree.pos.x * (int)tree.pos.y ^ 2);
            Quaternion rotation = Quaternion.Euler(0, rnd.Next(-180, 180), 0);

            Vector3 scale = new(2, 2, 2);

            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetTRS(localPosition, rotation, scale);
            combine[i].transform = matrix;
        }
        Mesh mesh = new()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        mesh.CombineMeshes(combine, false);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateUVDistributionMetrics();

        Debug.Log($"{map.trees.Count} => {mesh.vertices[0]}, {mesh.vertices[1]}, {mesh.vertices[2]}");

        return mesh;
    }
}
public class TreeMeshes
{
    public Dictionary<TreeTypes, List<Mesh>> treeMeshes = new();
    public Mesh GetMesh(TreeTypes treeType, int lod)
    {
        return treeMeshes[treeType][lod];
    }
}
[Serializable]
public class EditorTreeMeshes
{
    public EditorDictionary<TreeTypes, List<Mesh>> treeMeshes;

    public static explicit operator TreeMeshes(EditorTreeMeshes data)
    {
        TreeMeshes treeMeshes = new();

        foreach (SerializableKeyValuePair<TreeTypes, List<Mesh>> pair in data.treeMeshes)
        {
            treeMeshes.treeMeshes[pair.Key] = pair.Value;
        }
        return treeMeshes;
    }
}
