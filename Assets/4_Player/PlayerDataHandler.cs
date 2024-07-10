using UnityEngine;
public class PlayerDataHandler : MonoBehaviour, IDataCallbackReceiver
{
    [SerializeField] Transform playerCamera;
    PlayerLook playerLookScript;
    private void Awake()
    {
        MainSystem.gameState = GameState.MainMenu;
        MainSystem.StartGame += StartGame;

        Time.timeScale = 0;
    }
    private void OnDisable()
    {
        MainSystem.StartGame -= StartGame;
    }
    public void LoadData(WorldData worldData)
    {
        playerLookScript = GetComponent<PlayerLook>();

        PlayerData playerData = worldData.playerData;

        transform.position = playerData.position;
        playerLookScript.xRotation = playerData.rotation.x;
        transform.localRotation = Quaternion.Euler(0, playerData.rotation.y, playerData.rotation.z);
    }
    public void StartGame()
    {
        Debug.Log("StartGame");
        Time.timeScale = 1;

        if (Mathf.Round(transform.position.y) == 200)
            transform.position = new(0, MapDataHandler.chunks[new(0, 0)].map[120, 120].height + 5, 0);
    }

    public void SaveData(WorldData worldData)
    {
        PlayerData playerData = worldData.playerData;

        playerData.position = transform.position;
        playerData.rotation = new(playerLookScript.xRotation, transform.eulerAngles.x, transform.eulerAngles.z);

        worldData.playerData = playerData;
    }

    private void OnApplicationQuit()
    {
        MainSystem.ChangeGameState(GameState.Quitting);
    }
}
