using TMPro;
using UnityEngine;

public class DebugScreen : MonoBehaviour
{
    [SerializeField] TMP_Text seedText;
    [SerializeField] TMP_Text positionText;

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
    }
}
