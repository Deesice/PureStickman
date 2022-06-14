using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    static DifficultyManager instance;
    [Range(0, 1)]
    [SerializeField] float debugDifficulty;
    [SerializeField] GoalType debugGoalType;
    public event System.Action<float> DifficultyChanged;
    void Awake()
    {
        instance = this;
    }
    public static float GetDifficultyGradient()
    {
        var difficulty = District.DifficultyGradient;
        if (difficulty < 0)
            return instance.debugDifficulty;
        else
            return difficulty;
    }
    public static void AddDifficulty(float value)
    {
        if (value == 0)
            return;

        SetDifficulty(instance.debugDifficulty + value);
    }
    public static void SetDifficulty(float newDifficulty)
    {
        newDifficulty = Mathf.Clamp(newDifficulty, 0, 1);
        if (newDifficulty == instance.debugDifficulty)
            return;

        instance.debugDifficulty = newDifficulty;
        instance.DifficultyChanged?.Invoke(instance.debugDifficulty);
    }
    public static GoalType GetGoalType()
    {
        if (QuestView.currentQuest)
            return QuestView.currentQuest.missionType;
        else
            return instance.debugGoalType;
    }
    public static void Subscribe(System.Action<float> a)
    {
        instance.DifficultyChanged += a;
    }
}
