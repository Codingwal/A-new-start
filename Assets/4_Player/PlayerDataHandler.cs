using UnityEngine;
public class PlayerDataHandler : MonoBehaviour, IDataCallbackReceiver
{
    [SerializeField] Transform playerCamera;
    private PlayerLook playerLookScript;
    private void Awake()
    {
        MainSystem.gameState = GameState.MainMenu;
        MainSystem.StartGame += StartGame;

        Time.timeScale = 0;
    }
    private void Start()
    {
        MainSystem.ChangeGameState(GameState.InGame);
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
        transform.position += new Vector3(0, 10, 0);    // Spawn player a bit above the ground to prevent glitching through the mesh while the world is loaded
        playerLookScript.xRotation = playerData.rotation.x;
        transform.localRotation = Quaternion.Euler(0, playerData.rotation.y, playerData.rotation.z);
    }
    public void StartGame()
    {
        Debug.Log("StartGame");
        Time.timeScale = 1;
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
