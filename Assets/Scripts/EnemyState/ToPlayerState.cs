using UnityEngine;

class ToPlayerState : EnemyState
{
    float currentSpeed;
    public ToPlayerState(Enemy enemy) : base(enemy, true) { }

    public override void OnAnimationOver(string info)
    {
    }

    public override void OnEnter()
    {
        currentSpeed = 0;
    }

    public override void OnExit()
    {
    }
    public override void OnUpdate()
    {
        if (currentSpeed != host.CalculateSpeed())
        {
            float acceleration;
            if (currentSpeed < host.CalculateSpeed())
            {
                acceleration = host.Acceleration;
            }
            else
            {
                acceleration = host.Deacceleration;
            }
            currentSpeed = Mathf.Lerp(currentSpeed,
                host.CalculateSpeed(),
                Time.deltaTime * acceleration / Mathf.Abs(currentSpeed - host.CalculateSpeed()));
        }
        if (currentSpeed == 0)
            return;

        var toTarget = host.ToNearestTarget(out var eatable);
        var magnitude = toTarget.magnitude;
        toTarget = Vector3.Slerp(toTarget, new Vector3(Mathf.Sign(toTarget.x) * magnitude, 0, 0),
            Mathf.Abs(toTarget.x / toTarget.z));
        if (magnitude > (eatable ? 0.1f : host.DistantToAttack))
        {
            //transform.position = Vector3.Lerp(transform.position, transform.position + host.ToPlayer, currentSpeed * Time.deltaTime / magnitude);
            host.SetVelocity(currentSpeed * host.transform.forward);
            host.Animator.SetFloat("speed", currentSpeed * host.AnimationSpeedScale);

            var wantedRotation = Quaternion.LookRotation(toTarget);
            var angle = Quaternion.Angle(host.rotation, wantedRotation);
            if (angle > 0)
            {
                host.SetRotation(Quaternion.Slerp(host.rotation,
                    wantedRotation,
                    Time.deltaTime * host.RotationSpeed / angle));
            }
        }
        else
        {
            if (!GoalBase.Instance.IsComplete)
            {
                if (eatable)
                    host.ChangeState(new EatState(host));
                else
                    host.ChangeState(new AttackState(host));
            }
        }
    }
}
