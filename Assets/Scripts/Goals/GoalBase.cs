using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GoalType { Kills, StayAtPosition, Escort, Chase }

public abstract class GoalBase : MonoBehaviour
{
    [SerializeField] float _targetValue;
    public float TargetValue => _targetValue;
    public event Action MissionCompleted;
    public event Action MissionFailed;
    float _currentValue;
    protected float CurrentValue => _currentValue;
    public static GoalBase Instance { get; private set; }
    public bool IsComplete => _currentValue >= _targetValue;
    public float Progress => (_currentValue + 1) / _targetValue;
    public bool IsFailed { get; private set; }
    AudioSource source;
    [SerializeField] Sound failSound;
    [SerializeField] Sound completeSound;
    //[Header("Resurrection rules")]
    //[SerializeField] bool _tpPlayerToZeroPoint = true;
    //public bool TpPlayerToZeroPoint => _tpPlayerToZeroPoint;
    //[SerializeField] bool _prewarmFarm = true;
    //public bool PrewarmFarm => _prewarmFarm;
    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        Instance = this;
        GAManager.Instance?.SendProgressionEvent(GameAnalyticsSDK.GAProgressionStatus.Start,
            SceneManager.GetActiveScene().name + "_" + QuestView.currentQuest?.missionType);
        FindObjectOfType<EndScreen>().Resurrected += () =>
        {
            IsFailed = false;
            source.Stop();
            OnPlayerResurrected();
        };
    }
    private void Start()
    {
        SetupFailCondition(() =>
        {
            if (IsFailed || IsComplete)
                return;

            GAManager.Instance?.SendProgressionEvent(GameAnalyticsSDK.GAProgressionStatus.Fail,
                SceneManager.GetActiveScene().name + "_" + QuestView.currentQuest?.missionType,
                Mathf.FloorToInt(_currentValue * 100));
            //GAManager.Instance?.SendEvent(QuestView.currentQuest.missionName + "_failed_" + QuestView.currentQuest.GetDifficulty(), _currentValue);
            MissionFailed?.Invoke();
            OnFailed();
            IsFailed = true;
            failSound.Play(source);
        });
        Init();
    }
    protected abstract void SetupFailCondition(Action a);
    protected abstract void Init();
    protected abstract void OnPlayerResurrected();
    public abstract GoalType GetGoalType();
    protected abstract void OnFailed();
    protected abstract void OnValueAdded(bool isGoalCompleted);
    public void AddValue(GoalType eventType, float value)
    {
        if (eventType != GetGoalType())
            return;

        if (IsComplete)
            return;

        _currentValue = Mathf.Clamp(_currentValue + value, 0, _targetValue);
        if (IsComplete)
        {
            OnValueAdded(true);
            OnMissionComplete();
        }
        else
        {
            OnValueAdded(false);
        }
    }
    void OnMissionComplete()
    {
        if (IsFailed)
            return;

        MissionCompleted?.Invoke();
        GAManager.Instance?.SendProgressionEvent(GameAnalyticsSDK.GAProgressionStatus.Complete,
                SceneManager.GetActiveScene().name + "_" + QuestView.currentQuest?.missionType);
        //GAManager.Instance?.SendEvent(QuestView.currentQuest.missionName + "_complete_" + QuestView.currentQuest.GetDifficulty());
        Curtain.Instance.SetColor(MissionManager.CurtainVictoryColor);
        Curtain.Instance.Close(1, EndScreen.Instance.ShowReward);
        if (completeSound)
            completeSound.Play(source);
    }
}
