using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    static SaveManager _instance;
    static SaveManager Instance { get
        {
            if (_instance == null)
                new GameObject("SaveManager").AddComponent<SaveManager>();
            return _instance;
        } }
    void Awake()
    {
        if (_instance)
        {
            DestroyImmediate(this);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
