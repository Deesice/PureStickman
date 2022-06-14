using System;
using System.Collections.Generic;
using UnityEngine;

public class ZombieTarget : MonoBehaviour
{
    [SerializeField] Transform hips;
    public Vector3 HipsPosition => hips.position;
    public Func<bool> Eatable;
    public static List<Transform> AllTargets = new List<Transform>();
    private void Awake()
    {
        if (!hips)
            hips = transform;
        Eatable = () => false;
        AllTargets.Add(transform);
    }
    private void OnDestroy()
    {
        AllTargets.Remove(transform);
    }
}
