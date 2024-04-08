using UnityEngine;
public class PlayerDataHandler : MonoBehaviour, IDataCallbackReceiver
{
    [SerializeField] Transform playerCamera;
    private PlayerLook playerLookScript;
    private void Awake()
    {
        MainSystem.gameState = GameState.MainMenu;
        MainSystem.ChangeGameState(GameState.InGame);

        playerLookScript = GetComponent<PlayerLook>();

    }
    public void LoadData(WorldData worldData)
    {
        PlayerData playerData = worldData.playerData;

        transform.position = playerData.position;
        playerLookScript.xRotation = playerData.rotation.x;
        transform.localRotation = Quaternion.Euler(0, playerData.rotation.y, playerData.rotation.z);
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
