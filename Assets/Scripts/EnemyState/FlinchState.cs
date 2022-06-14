using UnityEngine;

class FlinchState : EnemyState
{
    float timeToNextState;
    public FlinchState(Enemy enemy) : base(enemy, true) { }
    public override void OnAnimationOver(string info)
    {
    }

    public override void OnEnter()
    {
        host.Animator.SetFloat("speed", 0);
        host.Animator.SetBool("flinch", true);
        timeToNextState = 0.25f;
    }

    public override void OnExit()
    {
        host.Animator.SetBool("flinch", false);
    }

    public override void OnUpdate()
    {
        timeToNextState -= Time.deltaTime;
        if (timeToNextState <= 0)
        {
            host.ChangeState(new ToPlayerState(host));
        }
    }
}
