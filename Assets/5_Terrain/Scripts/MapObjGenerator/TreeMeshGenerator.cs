using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class TreeMeshGenerator
{
    public static Mesh CreateTreeMesh(ChunkData map, int lod, TreeMeshes treeMeshes)
    {
        int halfChunkSize = MapGenerator.Instance.chunkSize / 2;

        // Create a list for each material (number of treeTypes * materials per treeType (trunk & leaf))
        List<CombineInstance>[] meshes = new List<CombineInstance>[treeMeshes.treeMeshes.Count * 2];

        // Intialize all lists
        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i] = new();
        }


        // Foreach tree...
        for (int i = 0; i < map.trees.Count; i++)
        {
            TreeData tree = map.trees[i];

            Vector3 localPosition = new(tree.pos.x, map.map[Mathf.RoundToInt(tree.pos.x + halfChunkSize), Mathf.RoundToInt(tree.pos.y + halfChunkSize)].height - 0.3f, tree.pos.y);

            // Gives the object a random but deterministic rotation
            System.Random rnd = new((int)tree.pos.x * (int)tree.pos.y ^ 2);
            Quaternion rotation = Quaternion.Euler(0, rnd.Next(-180, 180), 0);

            Vector3 scale = new(2, 2, 2);

            Matrix4x4 transform = Matrix4x4.identity;
            transform.SetTRS(localPosition, rotation, scale);

            CombineInstance trunkCombineInstance = new()
            {
                mesh = treeMeshes.GetMesh(tree.type, lod).trunkMesh,
                transform = transform
            };
            CombineInstance leafCombineInstance = new()
            {
                mesh = treeMeshes.GetMesh(tree.type, lod).leafMesh,
                transform = transform
            };

            // Index = treeTypeIndex * indicesPerTreeType [1 for trunk, 1 for leaf] + 1 if its the leafMesh
            Debug.Assert(meshes[(int)tree.type * 2] != null, "1");
            // Debug.Assert(trunkCombineInstance != null, "2");
            meshes[(int)tree.type * 2].Add(trunkCombineInstance);
            meshes[(int)tree.type * 2 + 1].Add(leafCombineInstance);
        }

        // Generate a default Matrix which is needed for the CombineInstances of the trunkMesh & leafMesh
        Matrix4x4 defaultTransform = Matrix4x4.identity;
        defaultTransform.SetTRS(new(0, 0, 0), Quaternion.identity, new(1, 1, 1));

        // Combine all meshes into one mesh for each material
        CombineInstance[] combineInstances = new CombineInstance[meshes.Length];
        for (int i = 0; i < meshes.Length; i++)
        {
            Mesh tmpMesh = new() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            tmpMesh.CombineMeshes(meshes[i].ToArray());
            combineInstances[i].mesh = tmpMesh;
            combineInstances[i].transform = defaultTransform;
        }

        // Combine the different meshes (one for each material) into one mesh but keep them seperated using submeshes
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