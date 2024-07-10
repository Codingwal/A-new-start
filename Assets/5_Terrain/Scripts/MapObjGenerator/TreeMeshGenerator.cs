using System;
using System.Collections.Generic;
using UnityEngine;

public static class TreeMeshGenerator
{
    public static Mesh CreateTreeMesh(ChunkData map, int lod, TreeMeshes treeMeshes)
    {
        int halfChunkSize = MapGenerator.Instance.chunkSize / 2;

        CombineInstance[] trunks = new CombineInstance[map.trees.Count];
        CombineInstance[] leafs = new CombineInstance[map.trees.Count];
        for (int i = 0; i < map.trees.Count; i++)
        {
            TreeData tree = map.trees[i];

            trunks[i].mesh = treeMeshes.GetMesh(tree.type, lod).trunkMesh;
            leafs[i].mesh = treeMeshes.GetMesh(tree.type, lod).leafMesh;

            Vector3 localPosition = new(tree.pos.x, map.map[Mathf.RoundToInt(tree.pos.x + halfChunkSize), Mathf.RoundToInt(tree.pos.y + halfChunkSize)].height - 0.3f, tree.pos.y);

            // Gives the object a random but deterministic rotation
            System.Random rnd = new((int)tree.pos.x * (int)tree.pos.y ^ 2);
            Quaternion rotation = Quaternion.Euler(0, rnd.Next(-180, 180), 0);

            Vector3 scale = new(2, 2, 2);

            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetTRS(localPosition, rotation, scale);
            trunks[i].transform = matrix;
            leafs[i].transform = matrix;
        }

        CombineInstance[] combine = new CombineInstance[2];

        Mesh trunkMesh = new() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        trunkMesh.CombineMeshes(trunks);
        trunkMesh.RecalculateBounds();
        combine[0].mesh = trunkMesh;
        Matrix4x4 matrix2 = Matrix4x4.identity;
        matrix2.SetTRS(new(0, 0, 0), Quaternion.identity, new(1, 1, 1));
        combine[0].transform = matrix2;

        Mesh leafMesh = new() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        leafMesh.CombineMeshes(leafs);
        trunkMesh.RecalculateBounds();
        combine[1].mesh = leafMesh;
        combine[1].transform = matrix2;
        Debug.Log($"{leafMesh.vertices[0]} {leafMesh.vertices[1]} {leafMesh.vertices[2]} {leafMesh.vertices[3]}");

        Debug.Log($"{combine[0].mesh.vertices[0]} {combine[0].mesh.vertices[1]} {combine[0].mesh.vertices[2]} {combine[0].mesh.vertices[3]}");
        Mesh mesh = new() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.CombineMeshes(combine, false);
        // mesh.RecalculateBounds();
        mesh.bounds = new(new(0, 0, 0), new(240, 500, 240));

        Debug.Log($"{mesh.vertices[0]} {mesh.vertices[1]} {mesh.vertices[2]} {mesh.vertices[3]}");


        Debug.Log($"{map.trees.Count}");

        return mesh;
    }
}
public class TreeMeshes
{
    public Dictionary<TreeTypes, List<TreeMesh>> treeMeshes = new();
    public TreeMesh GetMesh(TreeTypes treeType, int lod)
    {
        return treeMeshes[treeType][lod];
    }
}
[Serializable]
public class EditorTreeMeshes
{
    public EditorDictionary<TreeTypes, List<TreeMesh>> treeMeshes;

    public static explicit operator TreeMeshes(EditorTreeMeshes data)
    {
        TreeMeshes treeMeshes = new();

        foreach (SerializableKeyValuePair<TreeTypes, List<TreeMesh>> pair in data.treeMeshes)
        {
            treeMeshes.treeMeshes[pair.Key] = pair.Value;
        }
        return treeMeshes;
    }
}
[Serializable]
public class TreeMesh
{
    public Mesh trunkMesh;
    public Mesh leafMesh;
}