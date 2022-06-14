using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalReach : GoalBase
{
    public override GoalType GetGoalType()
    {
        return GoalType.Chase;
    }

    protected override void Init()
    {
    }

    protected override void OnFailed()
    {
    }

    protected override void OnPlayerResurrected()
    {
    }

    protected override void OnValueAdded(bool isGoalCompleted)
    {
    }

    protected override void SetupFailCondition(Action a)
    {
        FindObjectOfType<Player>().Dead += a;
    }
}
