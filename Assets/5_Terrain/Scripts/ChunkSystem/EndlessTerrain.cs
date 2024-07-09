using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject terrainChunkPrefab;
    public List<SerializableKeyValuePair<TreeTypes, GameObject>> treePrefabs;

    // Reduces the amount of chunk updates through a threshhold that the player needs to move
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    [SerializeField] LODInfo[] detailLevels;
    public static float maxViewDistance;


    // Reference to the player (The object that should be in the center of the loaded chunks)
    [SerializeField] Transform viewer;
    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    [SerializeField] Material mapMaterial;

    int chunkSize;
    int chunksVisibleInViewDistance;

    public static Dictionary<Vector2, TerrainChunk> terrainChunkDictonary = new();
    public static List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

    bool startedGame = false;
    public static int chunksWaitingForMapDataCount = 0;

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
                    terrainChunkDictonary.Add(chunkPos, new TerrainChunk(chunkPos, chunkSize, detailLevels, transform, mapMaterial, terrainChunkPrefab,
                                                                        SerializableKeyValuePair<TreeTypes, GameObject>.ToSerializableDictionary(treePrefabs)));
                }
            }
        }
    }
}
