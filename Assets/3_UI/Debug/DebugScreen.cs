using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugScreen : MonoBehaviour
{
    [SerializeField] TMP_Text seedText;
    [SerializeField] TMP_Text positionText;
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
    }
    void OnDestroy()
    {
        InputManager.ToggleDebug -= ToggleDebug;
        InputManager.ToggleDevSprint -= ToggleDevSprint;
        InputManager.DevJump -= DevJump;
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
        player.position = new(player.position.x, 200, player.position.z);
    }
    void Update()
    {
        int seed = MapDataHandler.worldData.terrainData.seed;
        seedText.text = $"Seed: {seed}";

        Vector3 position = player.position;
        positionText.text = $"Position: {position}";

        UpdateFPS();

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
