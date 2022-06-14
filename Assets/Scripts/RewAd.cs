using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class RewAd : MonoBehaviour
{
    [SerializeField] string rewardedUnitId;

    private RewardedAd rewardedAd;
    public static event Action Rewarded;

    private string testRewardedUnitId = "ca-app-pub-3940256099942544/5224354917";
    Canvas canvas;
    static RewAd instance;
    private void Awake()
    {
        if (instance)
        {
            DestroyImmediate(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void Init()
    {
#if UNITY_EDITOR
        rewardedUnitId = testRewardedUnitId;
#endif
        rewardedAd = new RewardedAd(rewardedUnitId);
        rewardedAd.OnAdClosed += Load;
        rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        Load(null, null);
    }
    private void Load(object sender, EventArgs e)
    {
        if (canvas)
            canvas.enabled = true;
        rewardedAd.Destroy();
        AdRequest adRequest = new AdRequest.Builder().Build();
        rewardedAd.LoadAd(adRequest);
    }

    private void HandleUserEarnedReward(object sender, Reward e)
    {
        SmartInvoke.Invoke(() => Rewarded?.Invoke(), 0.1f);
    }

    public static void ShowAd(Canvas canvasNeedToHide)
    {
        switch (Application.systemLanguage)
        {
#if !UNITY_EDITOR
            case SystemLanguage.Russian:
                UnityAdsManager.ShowRewarded();
                return;
#endif
            default:
                if (instance.rewardedAd.IsLoaded())
                {
                    instance.canvas = canvasNeedToHide;
                    if (instance.canvas)
                        instance.canvas.enabled = false;
                    instance.rewardedAd.Show();
                }
                return;
        }
    }
}
