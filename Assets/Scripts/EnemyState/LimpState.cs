using UnityEngine;

class LimpState : EnemyState
{
    float currentSpeed;
    float timeToDisableRagdoll;
    bool ragdoll;
    public LimpState(Enemy enemy) : base(enemy, false) { }
    public override void OnAnimationOver(string info)
    {
    }

    public override void OnEnter()
    {
        ragdoll = true;
        host.Animator.SetBool("limp", true);
        host.Ragdoll.EnableRagdoll();
        currentSpeed = 0;
        timeToDisableRagdoll = 2;
    }

    public override void OnExit()
    {
        host.Ragdoll.DisableRagdoll("limp", false);
        host.Animator.SetBool("limp", false);
    }

    public override void OnUpdate()
    {
        if (ragdoll)
        {
            timeToDisableRagdoll -= Time.deltaTime;
            if (timeToDisableRagdoll <= 0)
            {
                host.Ragdoll.DisableRagdoll("limp", true);
                ragdoll = false;
            }
            return;
        }

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

        var toTarget = host.ToNearestTarget();
        var magnitude = toTarget.magnitude;
        if (magnitude > host.DistantToAttack)
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
    }
}
