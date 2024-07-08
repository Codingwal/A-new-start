using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugScreen : MonoBehaviour
{
    [SerializeField] TMP_Text seedText;
    [SerializeField] TMP_Text positionText;
    [SerializeField] TMP_Text chunkText;
    [SerializeField] TMP_Text biomeText;
    [SerializeField] TMP_Text fpsText;
    [SerializeField] TMP_Text devSprintEnabledText;

    // Used to get player information
    [SerializeField] Transform player;
    [SerializeField] PlayerMovement playerMovementComponent;

    // Used to calc fps
    List<float> lastTimes = new();
    [SerializeField] int storedTimesCount = 20;
    int counter = 0;

    // devSprint (super fast sprint for devs)
    bool devSprintEnabled = false;
    [SerializeField] float devSprintSpeedMultiplier = 4;

    DebugScreen()
    {
        InputManager.ToggleDebug += ToggleDebug;
        InputManager.ToggleDevSprint += ToggleDevSprint;
        InputManager.DevJump += DevJump;

        // As this object is often inactive, this event is used instead of OnDestroy()
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    void OnSceneUnloaded(Scene scene)
    {
        // Only execute the code if this object is closed, which only happens if the "Singleplayer" scene is closed
        if (scene.name != SceneSystem.Scenes.Singleplayer.ToString()) return;

        InputManager.ToggleDebug -= ToggleDebug;
        InputManager.ToggleDevSprint -= ToggleDevSprint;
        InputManager.DevJump -= DevJump;

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    void ToggleDebug()
    {
        // If the debug screen is opened but the game isn't started, abort
        if (!transform.gameObject.activeSelf && !MainSystem.startedGame) return;

        // Toggle the debug to the opposite state of the current state (off->on & on->off)
        transform.gameObject.SetActive(!transform.gameObject.activeSelf);
    }
    void ToggleDevSprint()
    {
        devSprintEnabled = !devSprintEnabled;

        if (devSprintEnabled)
            playerMovementComponent.sprintSpeed *= devSprintSpeedMultiplier;
        else
            playerMovementComponent.sprintSpeed /= devSprintSpeedMultiplier;
    }
    void DevJump()
    {
        player.position = new(player.position.x, 500, player.position.z);
    }
    void Update()
    {
        int seed = MapDataHandler.worldData.terrainData.seed;
        seedText.text = $"Seed: {seed}";

        Vector3 position = player.position;
        positionText.text = $"Position: {position}";

        int chunkSize = MapGenerator.Instance.chunkSize - 1;
        Vector2Int chunk = new Vector2Int(Mathf.RoundToInt(position.x / chunkSize), Mathf.RoundToInt(position.z / chunkSize)) * chunkSize;
        chunkText.text = $"Chunk: {chunk}";

        UpdateFPS();

        ChunkData chunkData = MapDataHandler.chunks[chunk];

        UpdateBiomes(position, chunk, seed, chunkSize);

        devSprintEnabledText.text = $"Developer sprint is {(devSprintEnabled ? "enabled" : "disabled")}";
    }
    void UpdateFPS()
    {
        if (counter == 0)
        {
            float sum = 0;
            foreach (float time in lastTimes)
            {
                sum += time;
            }

            float average = sum / lastTimes.Count;
            float fps = 1 / average;
            counter = storedTimesCount;

            fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
            lastTimes.Clear();
        }
        lastTimes.Add(Time.deltaTime);
        counter--;
    }
    void UpdateBiomes(Vector3 pos, Vector2Int chunk, int seed, int chunkSize)
    {
        int halfChunkSize = chunkSize / 2;
        TerrainSettings terrainSettings = MapDataHandler.worldData.terrainSettings;

        List<KeyValuePair<float, Biomes>> bottomLeft = VertexGenerator.GetBiomeNames(chunk + new Vector2Int(-halfChunkSize, -halfChunkSize), terrainSettings, seed);
        List<KeyValuePair<float, Biomes>> bottomRight = VertexGenerator.GetBiomeNames(chunk + new Vector2Int(halfChunkSize, -halfChunkSize), terrainSettings, seed);
        List<KeyValuePair<float, Biomes>> topLeft = VertexGenerator.GetBiomeNames(chunk + new Vector2Int(-halfChunkSize, halfChunkSize), terrainSettings, seed);
        List<KeyValuePair<float, Biomes>> topRight = VertexGenerator.GetBiomeNames(chunk + new Vector2Int(halfChunkSize, halfChunkSize), terrainSettings, seed);

        // Lerp between the biome names
        Dictionary<Biomes, float> biomeNames = new();

        float px = Mathf.InverseLerp(chunk.x - halfChunkSize, chunk.x + halfChunkSize, pos.x);
        float py = Mathf.InverseLerp(chunk.y - halfChunkSize, chunk.y + halfChunkSize, pos.y);
        AddNames(bottomLeft, (1 - px) * (1 - py));
        AddNames(bottomRight, px * (1 - py));
        AddNames(topLeft, (1 - px) * py);//
        AddNames(topRight, px * py);

        biomeText.text = $"Biomes:\n";
        foreach (KeyValuePair<Biomes, float> biome in biomeNames)
        {
            biomeText.text += $"{Mathf.RoundToInt(biome.Value * 100)}% {biome.Key}\n";
        }

        // The function used to add all biomeNames
        // As this function isn't used anywhere else, it is declared as a local function
        void AddNames(List<KeyValuePair<float, Biomes>> namesToAdd, float relevance)
        {
            foreach (KeyValuePair<float, Biomes> pair in namesToAdd)
            {
                float percentage = pair.Key * relevance;

                // If the biome wasn't already added to the dict, add it with the corresponding percentage
                // Else, add the percentage to the current percentage for that biome
                if (!biomeNames.TryAdd(pair.Value, percentage))
                    biomeNames[pair.Value] += percentage;
            }
        }
    }
}