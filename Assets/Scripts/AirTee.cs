using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirTee : ArrowTarget, IPool
{
    [SerializeField] float floatSpeed;
    new Rigidbody rigidbody;
    Vector3 initialBodyLocalPosition;
    Quaternion initialBodyLocalRotation;
    SpinAnimation spinAnimation;
    private void Awake()
    {
        spinAnimation = GetComponent<SpinAnimation>();
        rigidbody = GetComponentInChildren<Rigidbody>();
        initialBodyLocalPosition = rigidbody.transform.localPosition;
        initialBodyLocalRotation = rigidbody.transform.localRotation;
        OnTakeFromPool();
    }
    public override bool AddDamage(DamageType damageType, Vector3 force)
    {
        return AddDamage(null, force);
    }
    private void Update()
    {
        if (!spinAnimation.enabled)
        {
            transform.position += floatSpeed * Time.deltaTime * Vector3.up;
        }
    }

    public override bool AddDamage(Collider damagedCollider, Vector3 force)
    {
        if (spinAnimation.enabled)
        {
            AddDamage(force);
            return true;
        }
        else
        {
            return false;
        }
    }
    void AddDamage(Vector3 force)
    {
        rigidbody.transform.parent = null;
        rigidbody.isKinematic = false;
        Xp.Instance.SpawnXpCrystal(rigidbody.worldCenterOfMass, 1);
        spinAnimation.enabled = false;
        GoalBase.Instance.AddValue(GoalType.Kills, 1);
    }
    public void OnTakeFromPool()
    {
        spinAnimation.enabled = true;
        rigidbody.transform.parent = transform;
        rigidbody.transform.localPosition = initialBodyLocalPosition;
        rigidbody.transform.localRotation = initialBodyLocalRotation;
        rigidbody.isKinematic = true;
    }
}
