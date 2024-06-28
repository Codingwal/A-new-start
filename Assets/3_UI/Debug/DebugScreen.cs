using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugScreen : MonoBehaviour
{
    [SerializeField] TMP_Text seedText;
    [SerializeField] TMP_Text positionText;
    [SerializeField] TMP_Text fpsText;

    List<float> lastTimes = new();
    [SerializeField] int granularity = 5;
    int counter = 10;

    Transform player;
    DebugScreen()
    {
        InputManager.ToggleDebug += ToggleDebug;
    }
    void OnEnable()
    {
        player = GameObject.Find("Player").transform;
    }
    void OnDestroy()
    {
        InputManager.ToggleDebug -= ToggleDebug;
    }
    void ToggleDebug()
    {
        transform.gameObject.SetActive(!transform.gameObject.activeSelf);
    }
    void Update()
    {
        int seed = MapDataHandler.worldData.terrainData.seed;
        seedText.text = $"Seed: {seed}";

        Vector3 position = player.position;
        positionText.text = $"Position: {position}";

        if (counter == 0)
        {
            float sum = 0;
            foreach (float time in lastTimes)
            {
                sum += time;
            }

            float average = sum / lastTimes.Count;
            float fps = 1 / average;
            counter = granularity;

            fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
        }
        lastTimes.Add(Time.deltaTime);
        counter--;
    }
}
