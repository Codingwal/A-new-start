using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class  Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    /// <summary>
    ///  Do not override this. Use SingletonAwake instead.
    /// </summary>
    protected virtual void Awake()
    {
        if (this == null)
        {
            return;
        }
        Instance = this as T;
        if (Instance == null)
        {
            Debug.LogWarning("Instance == null");
        }

        SingletonAwake();
    }
    protected virtual void SingletonAwake() { }
}
public abstract class SingletonPersistant<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        base.Awake();
    }
}
