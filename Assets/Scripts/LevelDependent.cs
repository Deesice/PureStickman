using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDependent : MonoBehaviour
{
    [SerializeField] int minLevelToActive;
    void Start()
    {
        if (Xp.Instance.Level > minLevelToActive)
            gameObject.SetActive(false);
    }
}
