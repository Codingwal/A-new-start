using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;

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
[Serializable]
public class SerializableKeyValuePair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;
    public SerializableKeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
    public static SerializableDictonary<TKey, TValue> ToSerializableDictionary(List<SerializableKeyValuePair<TKey, TValue>> list)
    {
        SerializableDictonary<TKey, TValue> dict = new();
        foreach (SerializableKeyValuePair<TKey, TValue> pair in list)
        {
            if (!dict.TryAdd(pair.Key, pair.Value))
                Debug.LogError($"Key {pair.Key} (Element {dict.Count} in the list) is already present in the dictionary!");
        }
        return dict;
    }
}