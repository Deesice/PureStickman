using UnityEngine;

class EatState : EnemyState
{
    public EatState(Enemy enemy) : base(enemy, false) { }

    public override void OnAnimationOver(string info)
    {
    }

    public override void OnEnter()
    {
        host.Animator.SetBool("eat", true);
    }

    public override void OnExit()
    {
        host.Animator.SetBool("eat", false);
    }

    public override void OnUpdate()
    {
        return;
        var wantedRotation = Quaternion.LookRotation(host.ToNearestTarget());
        var angle = Quaternion.Angle(wantedRotation, host.rotation);
        if (angle > 0)
            host.SetRotation(Quaternion.Slerp(host.rotation, wantedRotation, Time.deltaTime * host.RotationSpeed / angle));
    }
}
