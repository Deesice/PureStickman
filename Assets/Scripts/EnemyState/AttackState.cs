using UnityEngine;

class AttackState : EnemyState
{
    int selectedAttackAnimIdx;
    public AttackState(Enemy enemy) : base(enemy, true) { }

    public override void OnAnimationOver(string info)
    {
        if (info.Equals("attack_1") || info.Equals("attack_2"))
            host.ChangeState(new ToPlayerState(host));
        else
            Debug.LogError("Attack state receive non-attack AnimationOver message");
    }

    public override void OnEnter()
    {
        host.Animator.ResetTrigger("resetAttack");
        selectedAttackAnimIdx = Random.Range(1, 3);
        host.ZombieSoundController.Attack(0.2f * selectedAttackAnimIdx);
        host.Animator.SetTrigger("attack" + selectedAttackAnimIdx);
    }

    public override void OnExit()
    {
        host.Animator.ResetTrigger("attack" + selectedAttackAnimIdx);
        host.Animator.SetTrigger("resetAttack");
    }

    public override void OnUpdate()
    {
        var wantedRotation = Quaternion.LookRotation(host.ToNearestTarget());
        var angle = Quaternion.Angle(host.rotation, wantedRotation);
        if (angle > 0)
        {
            host.SetRotation(Quaternion.Slerp(host.rotation,
                wantedRotation,
                Time.deltaTime * host.RotationSpeed / angle));
        }
    }
}
