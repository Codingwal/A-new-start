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
        Vector2Int chunk = new Vector2Int(Mathf.RoundToInt(position.x / chunkSize), Mathf.RoundToInt(position.y / chunkSize)) * chunkSize;
        chunkText.text = $"Chunk: {chunk}";

        UpdateFPS();

        ChunkData chunkData = MapDataHandler.chunks[chunk];

        List<KeyValuePair<float, Biomes>> biomeNames = VertexGenerator.GetBiomeNames(new(position.x, position.z), MapDataHandler.worldData.terrainSettings, seed);
        biomeText.text = $"Biomes:\n";
        foreach (KeyValuePair<float, Biomes> biome in biomeNames)
        {
            biomeText.text += $"{Mathf.RoundToInt(biome.Key * 100)}% {biome.Value}\n";
        }

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
}
