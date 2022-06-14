using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    public static EndScreen Instance { get; private set; }
    [SerializeField] GameObject[] onCompleteButtons;
    [SerializeField] GameObject[] onFailedButtons;
    [SerializeField] Text killsText;
    [SerializeField] Text surviveTimeText;
    [SerializeField] Text xpField;
    [SerializeField] float fadeSpeed;
    int killCount;
    int xpCount;
    float survivalTime;
    int adCount;
    [SerializeField] MaskableGraphic[] fadedMaskableGraphics;
    [Header("Reward")]
    [SerializeField] Image[] rewardImages;
    [SerializeField] Text xpRewardText;
    [SerializeField] Text moneyRewardText;
    [SerializeField] float rewardShakeTime;
    [SerializeField] float rewardShakeRadius;
    [SerializeField] float rewardDelayTime;
    CanvasScaler canvasScaler;
    bool countingSurvivalTime = true;
    public event Action Resurrected;
    IEnumerator coroutine = null;
    public void ShowRewardedAd()
    {
        RewAd.ShowAd(canvasScaler.GetComponent<Canvas>());
    }
    void OnRewardedShowed()
    {
        if (GoalBase.Instance.IsComplete)
        {
            adCount++;
            Xp.Instance.AddXp(xpCount, true);
            xpField.text = LangAdapter.FindEntry("Gameplay", "XpCount") + xpCount + " + " + xpCount * adCount;
        }
        else
        {
            Xp.Instance.XpChanged += AddXpCount;
            countingSurvivalTime = true;
            Fade(0);
            Resurrected?.Invoke();
        }
    }
    private void Awake()
    {
        RewAd.Rewarded += OnRewardedShowed;
        UnityAdsManager.Rewarded += OnRewardedShowed;
        canvasScaler = GetComponentInParent<CanvasScaler>();
        Instance = this;

        foreach (var g in fadedMaskableGraphics)
        {
            var color = g.color;
            color.a = 0;
            g.color = color;
            g.enabled = false;
        }
    }
    private void OnDestroy()
    {
        RewAd.Rewarded -= OnRewardedShowed;
        UnityAdsManager.Rewarded -= OnRewardedShowed;
    }
    private void Start()
    {
        Xp.Instance.XpChanged += AddXpCount;
    }
    private void Update()
    {
        if (countingSurvivalTime)
            survivalTime += Time.deltaTime;
    }
    void AddXpCount(int value)
    {
        xpCount += value;
    }
    public void AddKill()
    {
        killCount++;
    }
    public void ShowReward()
    {
        Announcer.Instance.PlayImpactSound();
        if (QuestView.currentQuest && District.CurrentReward)
        {
            StartCoroutine(ShowingReward());
        }
        else
        {
            ShowStats();
        }
    }
    IEnumerator ShowingReward()
    {
        var startScale = 4.0f;
        var opacityColor = new Color(1, 1, 1, 0);
        var initialColors = new Color[rewardImages.Length];
        for (int j = 0; j < rewardImages.Length; j++)
        {
            rewardImages[j].enabled = true;
            initialColors[j] = rewardImages[j].color;
            initialColors[j].a = 1;
            rewardImages[j].color = opacityColor;
            rewardImages[j].rectTransform.localScale = Vector3.one * startScale;
        }

        moneyRewardText.text = "+ " + District.CurrentReward.money;
        moneyRewardText.enabled = true;
        moneyRewardText.color = opacityColor;
        moneyRewardText.rectTransform.localScale = Vector3.one * startScale;
        xpRewardText.text = "+ " + (QuestView.currentQuest.IsCompleted ? 0 : District.CurrentReward.xp);
        xpRewardText.enabled = true;
        xpRewardText.color = opacityColor;
        xpRewardText.rectTransform.localScale = Vector3.one * startScale;

        float i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime * fadeSpeed;
            for (int j = 0; j < rewardImages.Length; j++)
            {
                rewardImages[j].color = Color.Lerp(opacityColor, initialColors[j], i);
                rewardImages[j].rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 1, i);
            }
            moneyRewardText.color = Color.Lerp(opacityColor, Color.white, i);
            moneyRewardText.rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 1, i);
            xpRewardText.color = Color.Lerp(opacityColor, Color.white, i);
            xpRewardText.rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 1, i);
        }
        i = 0;
        var imagePoses = new Vector3[rewardImages.Length];
        for (int j = 0; j < imagePoses.Length; j++)
        {
            imagePoses[j] = rewardImages[j].rectTransform.position;
        }

        var moneyTextPos = moneyRewardText.rectTransform.position;
        var xpTextPos = xpRewardText.rectTransform.position;
        var outlines = new Outline[2];
        outlines[0] = moneyRewardText.GetComponent<Outline>();
        outlines[1] = xpRewardText.GetComponent<Outline>();
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / rewardShakeTime;
            var sin = Mathf.Sin(i * Mathf.PI * 2);
            var random = UnityEngine.Random.Range(0, Mathf.PI * 2);
            for (int j = 0; j < rewardImages.Length; j++)
            {
                rewardImages[j].rectTransform.position = imagePoses[j]
                    + new Vector3(Mathf.Cos(random), Mathf.Sin(random), 0)
                    * Mathf.Lerp(0, rewardShakeRadius * canvasScaler.GetRelative4K(), sin);
            }

            moneyRewardText.rectTransform.position = moneyTextPos
                + new Vector3(Mathf.Cos(random), Mathf.Sin(random), 0)
                * Mathf.Lerp(0, rewardShakeRadius * canvasScaler.GetRelative4K(), sin);

            xpRewardText.rectTransform.position = xpTextPos
                + new Vector3(Mathf.Cos(random), Mathf.Sin(random), 0)
                * Mathf.Lerp(0, rewardShakeRadius * canvasScaler.GetRelative4K(), sin);

            foreach (var o in outlines)
                o.effectColor = Color.Lerp(Color.black, Color.white, sin);
        }
        i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / rewardDelayTime;
        }
        i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime * fadeSpeed;
            moneyRewardText.color = Color.Lerp(opacityColor, Color.white, 1 - i);
            moneyRewardText.rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 1, 1 - i);
            xpRewardText.color = Color.Lerp(opacityColor, Color.white, 1 - i);
            xpRewardText.rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 1, 1 - i);
            for (int j = 0; j < rewardImages.Length; j++)
            {
                rewardImages[j].color = Color.Lerp(opacityColor, initialColors[j], 1 - i);
                rewardImages[j].rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 1, 1 - i);
            }
        }
        ApplyRewardToSpecs();
        QuestView.currentQuest.CompleteQuest();
        ShowStats();
    }
    void ApplyRewardToSpecs()
    {
        Inventory.Instance.AddShards(QuestView.currentQuest.missionType, District.CurrentReward.money);
        if (!QuestView.currentQuest.IsCompleted)
        {
            Xp.Instance.AddXp(District.CurrentReward.xp, true);
            xpCount -= District.CurrentReward.xp;
        }
    }
    public void ShowStats()
    {
        countingSurvivalTime = false;
        Xp.Instance.XpChanged -= AddXpCount;
        killsText.text = LangAdapter.FindEntry("Gameplay", "KillCount") + killCount;
        surviveTimeText.text = LangAdapter.FindEntry("Gameplay", "SurviveTimeCount") + survivalTime;
        xpField.text = LangAdapter.FindEntry("Gameplay", "XpCount") + xpCount;

        foreach (var i in onCompleteButtons)
            i.SetActive(GoalBase.Instance.IsComplete);

        foreach (var i in onFailedButtons)
            i.SetActive(!GoalBase.Instance.IsComplete);
        Fade(1);
    }
    public void ReloadCurrentScene()
    {
        ChangeScene(SceneManager.GetActiveScene().name);
    }
    public void ChangeScene(string sceneName)
    {
        if (Curtain.Instance.Opened)
        {
            Fade(0);
            Curtain.Instance.Close(Curtain.DefaultTime, () => SceneManager.LoadScene(sceneName));
        }
        else
        {
            Fade(0, () => SceneManager.LoadScene(sceneName));
        }
    }
    void Fade(float targetAlpha, Action callback = null)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = Fading(targetAlpha, callback);
        StartCoroutine(coroutine);
    }
    IEnumerator Fading(float targetAlpha, Action callback)
    {
        targetAlpha = Mathf.Clamp01(targetAlpha);
        bool flag = true;
        while (flag)
        {
            yield return null;
            flag = false;
            foreach (var m in fadedMaskableGraphics)
            {
                var curColor = m.color;
                var curAlpha = curColor.a;
                if (curAlpha != targetAlpha)
                {
                    curAlpha = Mathf.Lerp(curAlpha, targetAlpha, Time.deltaTime * fadeSpeed / Mathf.Abs(curAlpha - targetAlpha));
                    curColor.a = curAlpha;
                    m.color = curColor;
                    m.enabled = curAlpha > 0;
                    flag = true;
                }
            }
        }
        coroutine = null;
        callback?.Invoke();
    }
}
