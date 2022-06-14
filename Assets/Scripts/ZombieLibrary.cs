using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Custom/Zombie library", fileName = "NewZombieLibrary")]
public class ZombieLibrary : ScriptableObject
{
    public Enemy[] zombiePrefabs;
    public Enemy Random()
    {
        return zombiePrefabs.Random();
    }
    public Enemy Random(Predicate<Enemy> p)
    {
        return zombiePrefabs.Random(p);
    }
}
