using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyRegistrator
{
    static readonly List<Enemy> enemies = new List<Enemy>();
    public static bool IsContain(Predicate<Enemy> predicate)
    {
        foreach (var e in enemies)
            if (predicate(e))
                return true;
        return false;
    }
    public static void RegisterEnemy(Enemy e)
    {
        enemies.Add(e);
    }
    public static void ClearEnemy(Enemy e)
    {
        enemies.Remove(e);
    }
    public static Enemy GetNearestEnemy(float forwardPriority, List<Transform> targets, Predicate<Enemy> customPredicate = null)
    {
        float minValue = 1000000;
        Enemy nearest = null;
        foreach (var t in targets)
        {
            var e = GetNearestEnemy(forwardPriority, t, customPredicate);
            if (e)
            {
                var d = Mathf.Abs(t.position.z - e.position.z);
                if (d < minValue)
                {
                    minValue = d;
                    nearest = e;
                }
            }
        }
        return nearest;
    }
    public static Enemy GetNearestEnemy(float forwardPriority, Transform target, Predicate<Enemy> customPredicate = null)
    {
        float minValue = 1000000;
        Enemy nearest = null;
        foreach (var e in enemies)
        {
            if (!e.IsAlive || !e.Ragdoll.isKinematicNow || Mathf.Abs(target.position.y - e.transform.position.y) > 6)
                continue;

            if (customPredicate != null && !customPredicate(e))
                continue;

            var value = Mathf.Abs(target.position.z - e.transform.position.z)
                * ((e.transform.position.z - target.position.z) * Mathf.Sign(target.forward.z) > 0
                ? 1 : forwardPriority);
            if (value < minValue)
            {
                nearest = e;
                minValue = value;
            }
        }
        return nearest;
    }
}
