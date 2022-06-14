using UnityEngine;

class WakeUpState : EnemyState
{
    public WakeUpState(Enemy enemy) : base(enemy, false) { }
    public override void OnAnimationOver(string info)
    {
        if (info.Equals("wake_spine") || info.Equals("wake_stomach"))
            host.ChangeState(new ToPlayerState(host));
    }
    public override void OnEnter()
    {
        host.Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        host.Animator.SetFloat("speed", 0);
        if (host.GetDoubleTimeLegsDamageWhileDeadState)
        {
            host.Ragdoll.DisableRagdoll();
            host.ChangeState(new LimpState(host));
        }
        else
        {
            host.Ragdoll.DisableRagdoll(host.Ragdoll.IsLiesOnBelly ? "wake_stomach" : "wake_spine");
        }
    }

    public override void OnExit()
    {
        host.Animator.cullingMode = AnimatorCullingMode.CullCompletely;
    }

    public override void OnUpdate()
    {
    }
}
