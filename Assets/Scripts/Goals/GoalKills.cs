using System;
using UnityEngine;
using UnityEngine.UI;

public class GoalKills : GoalBase
{
    Text text;
    [SerializeField] string goalCategory;
    [SerializeField] string goalEntry;
    [SerializeField] bool infinityMode;
    [SerializeField] float increaseDifficultyValue;
    public override GoalType GetGoalType()
    {
        return GoalType.Kills;
    }
    protected override void OnFailed()
    {
        text.text = "";
    }
    protected override void OnPlayerResurrected()
    {
        UpdateText();
    }
    protected override void OnValueAdded(bool isGoalCompleted)
    {
        if (IsComplete)
            text.text = "";
        else
            UpdateText();
        DifficultyManager.AddDifficulty(increaseDifficultyValue);
    }
    void UpdateText()
    {
        text.text = LangAdapter.FindEntry(goalCategory, goalEntry) + CurrentValue + (infinityMode ? "" : "/" + TargetValue);
    }
    protected override void Init()
    {
        text = GetComponent<Text>();
        UpdateText();
    }

    protected override void SetupFailCondition(Action a)
    {
        FindObjectOfType<Player>().Dead += a;
    }
}
