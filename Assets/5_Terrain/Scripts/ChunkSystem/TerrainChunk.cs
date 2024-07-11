using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainChunk
{
    GameObject terrainChunk;
    Vector2Int position;
    Bounds bounds;

    // Trees
    Transform treeMeshChild;
    MeshFilter treeMeshFilter;
    MeshCollider treeMeshCollider;
    TreeMeshes treeMeshes;
    Mesh treeCollisionMeshPrefab;
    Mesh treeCollisionMesh;

    // River mesh
    Transform riverMeshChild;
    MeshFilter riverMeshFilter;

    // Terrain mesh
    Transform meshChild;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;

    ChunkData mapData;
    bool mapDataReceived;
    int previousLODIndex = -1;

    public TerrainChunk(Vector2Int coord, int size, LODInfo[] detailLevels, Transform parent, Material material, GameObject terrainChunkPrefab, TreeMeshes treeMeshes, Mesh treeCollisionMeshPrefab)
    {
        this.detailLevels = detailLevels;

        position = coord * size;
        bounds = new(new(position.x, position.y), Vector2.one * size);
        Vector3 positionV3 = new Vector3(position.x, 0, position.y) * MapGenerator.Instance.terrainSettings.uniformScale;

        // Instantiate the chunk using the terrainChunkPrefab
        terrainChunk = GameObject.Instantiate(terrainChunkPrefab, positionV3, Quaternion.identity, parent);

        meshChild = terrainChunk.transform.GetChild(0);

        // Get all required components
        meshRenderer = meshChild.GetComponent<MeshRenderer>();
        meshFilter = meshChild.GetComponent<MeshFilter>();
        meshCollider = meshChild.GetComponent<MeshCollider>();

        // Set the material containing the TerrainShader
        meshRenderer.material = material;

        // Get the object for the rivers and the meshFilter component attached to that child
        riverMeshChild = terrainChunk.transform.GetChild(1);
        riverMeshFilter = riverMeshChild.GetComponent<MeshFilter>();

        terrainChunk.transform.localScale = Vector3.one * MapGenerator.Instance.terrainSettings.uniformScale;

        // Hide this chunk until the mapData was received
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new(detailLevels[i], UpdateTerrainChunk);
        }

        // Initialize tree stuff
        this.treeMeshes = treeMeshes;
        treeMeshChild = terrainChunk.transform.GetChild(4);
        treeMeshFilter = treeMeshChild.GetComponent<MeshFilter>();
        treeMeshCollider = treeMeshChild.GetComponent<MeshCollider>();
        this.treeCollisionMeshPrefab = treeCollisionMeshPrefab;
        treeCollisionMesh = null;

        // Request the mapData and increase the counter used to determine the TerrainDataGeneration progress
        EndlessTerrain.chunksWaitingForMapDataCount++;
        MapGenerator.Instance.RequestMapData(position, OnMapDataReceived);
    }
    void OnMapDataReceived(ChunkData mapData)
    {
        this.mapData = mapData;
        mapDataReceived = true;

        UpdateTerrainChunk();

        // Decrease the counter used to determine the TerrainDataGeneration progress
        EndlessTerrain.chunksWaitingForMapDataCount--;
    }

    public void UpdateTerrainChunk()
    {
        if (!mapDataReceived)
        {
            return;
        }

        // If the player should see any point (even a small corner) of this chunk, set it to visible
        float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(EndlessTerrain.viewerPosition));
        bool visible = viewerDistanceFromNearestEdge <= EndlessTerrain.maxViewDistance;

        if (visible)
        {
            riverMeshFilter.mesh = RiverMeshGenerator.GenerateRiverMesh(mapData.rivers, 241);

            // Get the current detaillevel
            int lodIndex = 0;
            // Increase the lodIndex until the player is closer than the max distance for that LOD
            while (viewerDistanceFromNearestEdge > detailLevels[lodIndex].visibleDistanceThreshold)
            {
                lodIndex++;
            }

            // TODO: Can this cause bugs? If the detaillevel hasn't changed but a callback triggered this function after a mesh has been generated,
            // This code will be skipped

            // If the detaillevel changed...
            if (lodIndex != previousLODIndex)
            {
                // If the mesh for the new detaillevel already exists, use this mesh, else, if the mesh hasn't already been requested, do so
                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh)
                {
                    previousLODIndex = lodIndex;
                    meshFilter.mesh = lodMesh.terrainMesh;
                    treeMeshFilter.mesh = lodMesh.treeMesh;
                }
                else if (!lodMesh.hasRequestedMesh)
                {
                    lodMesh.RequestMesh(mapData, treeMeshes);
                }

                // If the chunk has the lowest detaillevel, add colliders..
                if (lodIndex == 0)
                {
                    // If the LOD0 mesh already exists, use it. If it doesn't, it has already been requested by the previous code
                    if (lodMesh.hasMesh)
                        meshCollider.sharedMesh = lodMesh.terrainMesh;

                    // If no tree collision mesh is present, generate it
                    if (treeCollisionMesh == null)
                    {
                        treeCollisionMesh = TreeMeshGenerator.CreateTreeColliderMesh(mapData, treeCollisionMeshPrefab);
                        Debug.Log("Generated tree collision mesh");
                    }

                    // Assign the treeCollisionMesh
                    treeMeshCollider.sharedMesh = treeCollisionMesh;
                    Debug.Log("Assigned treeCollisionMesh");
                }
            }

            EndlessTerrain.terrainChunksVisibleLastUpdate.Add(this);
        }

        SetVisible(visible);
    }
    public void SetVisible(bool visible)
    {
        terrainChunk.SetActive(visible);
    }
    public bool IsVisible()
    {
        return terrainChunk.activeSelf;
    }
}
[System.Serializable]
class LODMesh
{
    public Mesh terrainMesh;
    public Mesh treeMesh;
    public Mesh treeCollisionMesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    readonly LODInfo lodInfo; // Level of detail
    readonly System.Action updateCallback;

    public LODMesh(LODInfo lodInfo, System.Action updateCallback)
    {
        this.lodInfo = lodInfo;
        this.updateCallback = updateCallback;
    }
    void OnMeshDataReceived(MeshData terrainMeshData)
    {
        terrainMesh = terrainMeshData.CreateMesh();
        hasMesh = true;

        // Decrease the counter used to determine the TerrainGeneration progress
        EndlessTerrain.chunksWaitingForMeshCount--;

        updateCallback();
    }
    public void RequestMesh(ChunkData mapData, TreeMeshes treeMeshes)
    {
        // Incraese the counter used to determine the TerrainGeneration progress
        EndlessTerrain.chunksWaitingForMeshCount++;

        hasRequestedMesh = true;
        MapGenerator.Instance.RequestMeshData(mapData, lodInfo.lod, OnMeshDataReceived);

        // A treeLOD of -1 means that a tree mesh shouldn't be generated
        if (lodInfo.treeLOD == -1)
            treeMesh = new();
        else
            treeMesh = TreeMeshGenerator.CreateTreeMesh(mapData, lodInfo.treeLOD, treeMeshes);
    }
}
[System.Serializable]
public struct LODInfo
{
    public int lod;
    public float visibleDistanceThreshold;
    [Header("Use -1 if no trees should be visible")]
    public int treeLOD;
}
