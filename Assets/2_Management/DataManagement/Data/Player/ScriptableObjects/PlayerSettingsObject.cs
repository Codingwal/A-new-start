using System;
using UnityEngine;

[CreateAssetMenu()]
[Serializable]
public class PlayerSettingsObject : ScriptableObject
{
    public static explicit operator PlayerSettings(PlayerSettingsObject obj)
    {
        return new();
    }
}
