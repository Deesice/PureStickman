using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tee : ArrowTarget, IPool
{
    new Rigidbody rigidbody;
    PlayerWall playerWall;
    [SerializeField] Collider teeCollider;
    private void Awake()
    {
        playerWall = GetComponent<PlayerWall>();
        rigidbody = GetComponent<Rigidbody>();
        OnTakeFromPool();
    }
    public override bool AddDamage(DamageType damageType, Vector3 force)
    {
        return AddDamage(teeCollider, force);
    }
    public override bool AddDamage(Collider damagedCollider, Vector3 force)
    {
        if (playerWall.enabled && damagedCollider == teeCollider)
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
        Xp.Instance.SpawnXpCrystal(teeCollider.bounds.center, 1);
        playerWall.enabled = false;
        rigidbody.AddForce(force, ForceMode.Impulse);
        GoalBase.Instance.AddValue(GoalType.Kills, 1);
    }
    public void OnTakeFromPool()
    {
        playerWall.enabled = true;
    }
}
