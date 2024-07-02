using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public GameObject terrainChunkPrefab;

    // Reduces the amount of chunk updates through a threshhold that the player needs to move
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    [SerializeField] LODInfo[] detailLevels;
    public static float maxViewDistance;


    // Reference to the player (The object that should be in the center of the loaded chunks)
    [SerializeField] Transform viewer;
    [SerializeField] static Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    [SerializeField] Material mapMaterial;

    int chunkSize;
    int chunksVisibleInViewDistance;

    public static Dictionary<Vector2, TerrainChunk> terrainChunkDictonary = new();
    public static List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

    bool startedGame = false;
    static int chunksWaitingForMapDataCount = 0;

    private void Awake()
    {
        MainSystem.LoadWorld += LoadWorld;
    }

    private void OnDisable()
    {
        MainSystem.LoadWorld -= LoadWorld;
    }
    private void LoadWorld()
    {
        maxViewDistance = detailLevels[^1].visibleDistanceThreshold;
        chunkSize = MapGenerator.Instance.chunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        terrainChunksVisibleLastUpdate.Clear();
        terrainChunkDictonary.Clear();
        UpdateVisibleChunks();
    }
    private void Update()
    {
        if (!startedGame)
        {
            // This value is used by the SceneSystem to calculate the TerrainGeneration progress
            SceneSystem.chunksWaitingForMapDataCount = chunksWaitingForMapDataCount;

            // If all chunks rendered at game start have their mapData, start the game
            if (chunksWaitingForMapDataCount == 0)
            {
                MainSystem.StartGameplay();
                startedGame = true;
            }
        }

        // Only update chunks if the game is enabled, this loop isn't executed if the game is pause
        if (MainSystem.gameState != GameState.InGame)
        {
            return;
        }

        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        viewerPosition /= MapGenerator.Instance.terrainSettings.uniformScale;

        // If the player moved far enough, update the chunks
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }
    private void UpdateVisibleChunks()
    {
        // Deactivate all chunks which were visible last update
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        // Get the chunk in which the player is currently in (not in worldSpace!)
        Vector2Int playerChunk = Vector2Int.RoundToInt(viewerPosition / chunkSize);

        // For each point on the chunkGrid in the viewDistance
        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2Int chunkPos = new(playerChunk.x + xOffset, playerChunk.y + yOffset);

                // If the chunk already exists, update it, else, create a new chunk
                if (terrainChunkDictonary.ContainsKey(chunkPos))
                {
                    terrainChunkDictonary[chunkPos].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictonary.Add(chunkPos, new TerrainChunk(chunkPos, chunkSize, detailLevels, transform, mapMaterial, terrainChunkPrefab));
                }
            }
        }
    }

    [System.Serializable]
    public class TerrainChunk
    {
        GameObject terrainChunk;
        Vector2Int position;
        Bounds bounds;

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

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2Int coord, int size, LODInfo[] detailLevels, Transform parent, Material material, GameObject terrainChunkPrefab)
        {
            this.detailLevels = detailLevels;

            position = coord * size;
            bounds = new(new(position.x, position.y), Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y) * MapGenerator.Instance.terrainSettings.uniformScale;

            // Instantiate the chunk using the terrainChunkPrefab
            terrainChunk = Instantiate(terrainChunkPrefab, positionV3, Quaternion.identity, parent);

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

            // Request the mapData and increase the counter used to determine the TerrainGeneration progress
            chunksWaitingForMapDataCount++;
            MapGenerator.Instance.RequestMapData(position, OnMapDataReceived);
        }
        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            // Decrease the counter used to determine the TerrainGeneration progress
            chunksWaitingForMapDataCount--;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (!mapDataReceived)
            {
                return;
            }

            // If the player should see any point (even a small corner) of this chunk, set it to visible
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

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

                terrainChunksVisibleLastUpdate.Add(this);
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
        public void RequestMesh(MapData mapData)
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
}
