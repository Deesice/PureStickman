using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveLoadBehaviour<T> : MonoBehaviour where T: SaveLoadBehaviour<T>
{
    [SerializeField] bool dontDestroyObject;
    public static T Instance { get; private set; }
    void Awake()
    {
        if (Instance)
        {
            DestroyImmediate(this);
            return;
        }
        Instance = this as T;
        if (dontDestroyObject)
            DontDestroyOnLoad(gameObject);
        Load();
    }
    protected abstract void Load();
    protected abstract void Save();
    private void OnApplicationPause(bool pause)
    {
        if (pause)
            Save();
    }
    private void OnDestroy()
    {
        if (dontDestroyObject && Instance != this as T)
            return;
        
        Save();
    }
}
