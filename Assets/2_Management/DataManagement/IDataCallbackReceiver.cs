using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataCallbackReceiver
{
    void LoadData(WorldData worldData);
    void SaveData(WorldData worldData);
}
