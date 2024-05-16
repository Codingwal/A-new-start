using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ListWrapper<T>
{
    public List<T> list = new();
    public ListWrapper()
    {
        
    }
    public ListWrapper(List<T> list)
    {
        this.list = list;
    }
}
