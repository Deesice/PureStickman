using System;
using UnityEngine;
using UnityEngine.UI;

public class GoalFillBar : GoalBase
{
    [SerializeField] Text fillText;
    [SerializeField] Image fillImage;
    [SerializeField] Image backgroundImage;
    bool displayedValue;
    public override GoalType GetGoalType()
    {
        return GoalType.StayAtPosition;
    }
    protected override void OnFailed()
    {
        Hide();
    }
    protected override void OnPlayerResurrected()
    {
        Show();
    }
    protected override void OnValueAdded(bool isGoalCompleted)
    {
        if (IsComplete)
        {
            Hide();
        }
        else
        {
            if (displayedValue)
            {
                fillText.text = Mathf.FloorToInt(CurrentValue * 100 / TargetValue) + "%";
                fillImage.fillAmount = CurrentValue / TargetValue;
            }
        }
    }
    void Hide()
    {
        if (displayedValue)
        {
            fillText.enabled = false;
            fillImage.enabled = false;
            backgroundImage.enabled = false;
        }
    }
    void Show()
    {
        if (displayedValue)
        {
            fillText.enabled = true;
            fillImage.enabled = true;
            backgroundImage.enabled = true;
        }
    }
    protected override void Init()
    {
        displayedValue = fillText && fillImage && backgroundImage;
        if (displayedValue)
        {
            fillText.text = "0%";
            fillImage.fillAmount = 0;
        }
        else
        {
            if (fillText)
                fillText.enabled = false;
            if (fillImage)
                fillImage.enabled = false;
            if (backgroundImage)
                backgroundImage.enabled = false;
        }
    }
    protected override void SetupFailCondition(Action a)
    {
        FindObjectOfType<Player>().Dead += a;
    }
}
