using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class DeadState : EnemyState
{
    float timer;
    public DeadState(Enemy enemy) : base(enemy, false) { }
    public override void OnAnimationOver(string info)
    {
    }

    public override void OnEnter()
    {
        if (host.CurrentPhase != EnemyPhase.Dead)
        {
            host.Heal();
        }
        host.Ragdoll.EnableRagdoll();
        timer = 3;
    }    
    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (host.CurrentPhase != EnemyPhase.Dead)
            {
                host.ChangeState(new WakeUpState(host));
            }
            else
            {
                if (host.Rigidbodies.Contains(BodyPredicate))
                    return;

                Debug.Log(host.gameObject.name + " ragdoll disabled");
                timer = 1000000;
                host.Ragdoll.SetKinematic(true);
                host.DisableColliders(true);
            }
        }
    }
    bool BodyPredicate(Rigidbody r)
    {
        return !r.IsSleeping();
    }
}
