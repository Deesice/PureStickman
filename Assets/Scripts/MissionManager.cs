using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    [SerializeField] bool tutorial;
    static MissionManager instance;
    [SerializeField] UnityEvent[] goalDependentActions;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] Color _curtainVictoryColor;
    public static Color CurtainVictoryColor => instance._curtainVictoryColor;
    public static Vector3 SpawnPoint => instance.tutorial ? Vector3.zero : instance.spawnPoints[(int)DifficultyManager.GetGoalType()].position;
    void Awake()
    {
        instance = this;
        if (!tutorial)
        {
            goalDependentActions[(int)DifficultyManager.GetGoalType()].Invoke();
        }
    }
}
