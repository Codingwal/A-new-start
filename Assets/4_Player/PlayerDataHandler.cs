using UnityEngine;
public class PlayerDataHandler : MonoBehaviour, IDataCallbackReceiver
{
    [SerializeField] Transform playerCamera;
    private PlayerLook playerLookScript;
    private void Awake()
    {
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
}
