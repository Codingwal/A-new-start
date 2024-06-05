using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public GameObject terrainChunkPrefab;

    // Reduces the amount of chunk updates through a threshhold that the player needs to move
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    public LODInfo[] detailLevels;
    public static float maxViewDistance;


    // Reference to the player (The object that should be in the center of the loaded chunks)
    public Transform viewer;

    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    int chunkSize;
    int chunksVisibleInViewDistance;

    public static Dictionary<Vector2, TerrainChunk> terrainChunkDictonary = new();
    public static List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

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
        if (MainSystem.gameState != GameState.InGame)
        {
            return;
        }

        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        viewerPosition /= MapGenerator.Instance.terrainSettings.uniformScale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }
    private void UpdateVisibleChunks()
    {
        Debug.Log("Updating visible chunks");

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2Int viewedChunkCoord = new(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictonary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictonary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictonary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, terrainChunkPrefab));
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

            terrainChunk = Instantiate(terrainChunkPrefab, positionV3, Quaternion.identity, parent);

            meshChild = terrainChunk.transform.GetChild(1);

            meshRenderer = meshChild.GetComponent<MeshRenderer>();
            meshFilter = meshChild.GetComponent<MeshFilter>();
            meshCollider = meshChild.GetComponent<MeshCollider>();

            meshRenderer.material = material;

            terrainChunk.transform.localScale = Vector3.one * MapGenerator.Instance.terrainSettings.uniformScale;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new(detailLevels[i].lod, UpdateTerrainChunk);
                if (detailLevels[i].useForCollider)
                {
                    collisionLODMesh = lodMeshes[i];
                }
            }
            MapGenerator.Instance.RequestMapData(position, OnMapDataReceived);
        }
        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (!mapDataReceived)
            {
                return;
            }

            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

            if (visible)
            {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }
                if (lodIndex != previousLODIndex)
                {
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
                }

                if (lodIndex == 0)
                {
                    if (collisionLODMesh.hasMesh)
                    {
                        meshCollider.sharedMesh = collisionLODMesh.mesh;
                    }
                    else if (!collisionLODMesh.hasRequestedMesh)
                    {
                        collisionLODMesh.RequestMesh(mapData);
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
        int lod; // Level of detail
        System.Action updateCallback;

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
        public bool useForCollider;
    }
}
