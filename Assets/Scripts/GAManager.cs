using GameAnalyticsSDK;
using UnityEngine;

public class GAManager : MonoBehaviour
{
    public static GAManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        GameAnalytics.Initialize();
        SendEventOnce("first_launch");
        SendEvent("game_start");
    }
    public void SendEventOnce(string eventName)
    {
        if (PlayerPrefs.GetInt("GAnalytics-" + eventName, 0) == 0)
        {
            SendEvent(eventName);
            PlayerPrefs.SetInt("GAnalytics-" + eventName, 1);
        }
    }
    public void SendProgressionEvent(GAProgressionStatus status, string levelName)
    {
        GameAnalytics.NewProgressionEvent(status, levelName);
    }
    public void SendProgressionEvent(GAProgressionStatus status, string levelName, int score)
    {
        GameAnalytics.NewProgressionEvent(status, levelName, score);
    }
    public void SendEvent(string eventName)
    {
        GameAnalytics.NewDesignEvent(eventName);
    }
    public void SendEvent(string eventName, float eventValue)
    {
        GameAnalytics.NewDesignEvent(eventName, eventValue);
    }
}
