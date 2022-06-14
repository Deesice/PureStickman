using System;
public class GoalEscort : GoalBase
{
    public override GoalType GetGoalType()
    {
        return GoalType.Escort;
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
        foreach (var npc in FindObjectsOfType<NPC>())
        {
            npc.Dead += a;
        }
    }
}
