using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SerializableDictonary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();

    // Save the dictionary to lists
    public void OnAfterDeserialize()
    {
        Clear();

        if (keys.Count != values.Count)
        {
            Debug.LogError($@"Tried to deserialize a SerializableDictionary,
                            but the amount of keys ({keys.Count}) does not match the number of values ({values.Count}).");
        }

        for (int i = 0; i < keys.Count; i++)
        {
            this[keys[i]] = values[i];
        }
    }

    // load the dictionary from lists
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
}
