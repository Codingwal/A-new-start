using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class EditorDictionary<TKey, TValue> : IEnumerable<SerializableKeyValuePair<TKey, TValue>>
{
    public List<SerializableKeyValuePair<TKey, TValue>> data = new();

    public static explicit operator Dictionary<TKey, TValue>(EditorDictionary<TKey, TValue> editorDict)
    {
        Dictionary<TKey, TValue> dict = new();

        foreach (SerializableKeyValuePair<TKey, TValue> pair in editorDict.data)
        {
            if (!dict.TryAdd(pair.Key, pair.Value))
                Debug.LogError($"The key {pair.Key} is already present in the dictionary!");
        }
        return dict;
    }
    public IEnumerator<SerializableKeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return data.GetEnumerator();
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