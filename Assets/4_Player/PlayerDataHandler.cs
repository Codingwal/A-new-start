using UnityEngine;
public class PlayerDataHandler : MonoBehaviour, IDataCallbackReceiver
{
    [SerializeField] Transform playerCamera;
    private PlayerLook playerLookScript;
    private void Awake()
    {
        if (!TryGetComponent(out playerLookScript)) Debug.Log("WTF");

        MainSystem.gameState = GameState.MainMenu;
        MainSystem.ChangeGameState(GameState.InGame);


    }
    public void LoadData(WorldData worldData)
    {
        if (!TryGetComponent(out playerLookScript)) Debug.Log("WTF");

        PlayerData playerData = worldData.playerData;

        transform.position = playerData.position;
        if (playerLookScript == null) Debug.Log("player");
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
