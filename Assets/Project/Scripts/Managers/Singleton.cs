using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;
    public static T Instance { get => _instance; set => _instance = value; }
    protected virtual void Awake()
    {
        if (_instance)
            Destroy(gameObject);
        _instance = (T)this;
    }
}
