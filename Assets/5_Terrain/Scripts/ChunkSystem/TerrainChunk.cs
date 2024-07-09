using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainChunk
{
    GameObject terrainChunk;
    Vector2Int position;
    Bounds bounds;

    // Trees
    Transform treeChild;
    SerializableDictonary<TreeTypes, GameObject> treePrefabs;
    bool hasTrees = false;
    List<GameObject> trees;

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
    LODMesh collisionLODMesh;

    ChunkData mapData;
    bool mapDataReceived;
    int previousLODIndex = -1;

    public TerrainChunk(Vector2Int coord, int size, LODInfo[] detailLevels, Transform parent, Material material, GameObject terrainChunkPrefab, SerializableDictonary<TreeTypes, GameObject> treePrefabs)
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
            lodMeshes[i] = new(detailLevels[i].lod, UpdateTerrainChunk);
        }
        collisionLODMesh = lodMeshes[0];

        this.treePrefabs = treePrefabs;
        treeChild = terrainChunk.transform.GetChild(4);

        // Request the mapData and increase the counter used to determine the TerrainGeneration progress
        EndlessTerrain.chunksWaitingForMapDataCount++;
        MapGenerator.Instance.RequestMapData(position, OnMapDataReceived);
    }
    void OnMapDataReceived(ChunkData mapData)
    {
        this.mapData = mapData;
        mapDataReceived = true;

        // Decrease the counter used to determine the TerrainGeneration progress
        EndlessTerrain.chunksWaitingForMapDataCount--;

        UpdateTerrainChunk();
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
            if (!hasTrees)
            {
                trees = TreeObjGenerator.InstantiateTrees(mapData, treeChild, treePrefabs);
                hasTrees = true;
            }

            riverMeshFilter.mesh = RiverMeshGenerator.GenerateRiverMesh(mapData.rivers, 241);

            // Get the current detaillevel
            int lodIndex = 0;
            // Increase the lodIndex until the player is closer than the max distance for that LOD
            while (viewerDistanceFromNearestEdge > detailLevels[lodIndex].visibleDistanceThreshold)
            {
                lodIndex++;
            }

            // If the detaillevel changed...
            if (lodIndex != previousLODIndex)
            {
                // If the mesh for the new detaillevel already exists, use this mesh, else, if the mesh hasn't already been requested, do so
                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh)
                {
                    previousLODIndex = lodIndex;
                    meshFilter.mesh = lodMesh.mesh;
                }
                else if (!lodMesh.hasRequestedMesh)
                {
                    lodMesh.RequestMesh(mapData);
                }

                // If the chunk has the lowest detaillevel...
                if (lodIndex == 0)
                {
                    // If the collisionMesh already exists, use this mesh, else, if the mesh hasn't been requested, do so
                    if (collisionLODMesh.hasMesh)
                    {
                        meshCollider.sharedMesh = collisionLODMesh.mesh;
                    }
                    else if (!collisionLODMesh.hasRequestedMesh)
                    {
                        collisionLODMesh.RequestMesh(mapData);
                    }
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
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    readonly int lod; // Level of detail
    readonly System.Action updateCallback;

    public LODMesh(int lod, System.Action updateCallback)
    {
        this.lod = lod;
        this.updateCallback = updateCallback;
    }
    void OnMeshDataReceived(MeshData meshData)
    {
        mesh = meshData.CreateMesh();
        hasMesh = true;

        updateCallback();
    }
    public void RequestMesh(ChunkData mapData)
    {
        hasRequestedMesh = true;
        MapGenerator.Instance.RequestMeshData(mapData, lod, OnMeshDataReceived);
    }
}
[System.Serializable]
public struct LODInfo
{
    public int lod;
    public float visibleDistanceThreshold;
}
