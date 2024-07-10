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

        CombineInstance[] combineInstances = new CombineInstance[2];

        // Generate a default Matrix which is needed for the CombineInstances of the trunkMesh & leafMesh
        Matrix4x4 defaultMatrix = Matrix4x4.identity;
        defaultMatrix.SetTRS(new(0, 0, 0), Quaternion.identity, new(1, 1, 1));

        // Combine all trunkMeshes and add the result (trunkMesh) to the combineInstances array
        {
            Mesh trunkMesh = new() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            trunkMesh.CombineMeshes(trunks);
            combineInstances[0].mesh = trunkMesh;
            combineInstances[0].transform = defaultMatrix;
        }

        // Combine all leafMeshes and add the result (leafMesh) to the combineInstances array
        {
            Mesh leafMesh = new() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            leafMesh.CombineMeshes(leafs);
            combineInstances[1].mesh = leafMesh;
            combineInstances[1].transform = defaultMatrix;
        }

        // Combine the leafMesh and the trunkMesh into one mesh (keep them seperated using different subMeshes)
        Mesh mesh = new() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.CombineMeshes(combineInstances, false);

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