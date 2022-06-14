using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ArrowTarget : MonoBehaviour
{
    [SerializeField] bool stickToCenter;
    public bool StickToCenter => stickToCenter;
    SelfInitializingList<Collider> _colliders;
    public List<Collider> Colliders => _colliders.GetField(this.GetComponentsInChildren<Collider>);
    public abstract bool AddDamage(DamageType damageType, Vector3 force);
    public abstract bool AddDamage(Collider damagedCollider, Vector3 force);
}
public struct SelfInitializingList<T>
{
    bool initialized;
    List<T> _field;
    public List<T> GetField(Func<IEnumerable<T>> collectionGetter)
    {
        if (!initialized)
        {
            initialized = true;
            _field = new List<T>();
            _field.AddRange(collectionGetter());
        }
        return _field;
    }
}
