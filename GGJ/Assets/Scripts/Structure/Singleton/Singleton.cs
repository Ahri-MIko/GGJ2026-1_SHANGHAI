using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例模式基类，不继承Monobehavior
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : class, new()
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = new T();
            return instance;
        }
    }
}
