using UnityEngine;
public class PlayerData
{
    public Vector3 position;
    public Vector3 rotation;

    public PlayerData()
    {
        position = new(0, 200, 0);
        rotation = new();
    }

    public static PlayerData Default
    {
        get
        {
            return new()
            {
                position = new(0, 200, 0),
                rotation = new(),
            };
        }
    }
}