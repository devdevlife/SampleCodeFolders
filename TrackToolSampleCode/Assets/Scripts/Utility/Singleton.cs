using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class Singleton<T> where T : new()
{
    protected static T m_instance = new T();

    public void CreateInstance() { }

    public static T Instance
    {
        get
        {
            return m_instance;
        }
    }
}
