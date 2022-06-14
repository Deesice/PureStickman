using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public enum BubbleType { One, Two, Five, PercentFive }
public class Xp : SaveLoadBehaviour<Xp>
{
    public const int PointsPerLevel = 1500;
    public int Value { get; private set; }
    public int Level { get; private set; }
    public event Action<int> XpChanged;
    public event Action<int> LevelUp;
    [SerializeField] Text valueText;
    UIAnimationHelper valueTextAnimationHelper;
    [SerializeField] Image fillImage;
    [SerializeField] Sprite[] levelSprites;
    [Header("Bubble settings")]
    [SerializeField] Sprite[] bubbleSprites;
    [SerializeField] GameObject bubblePrefab;
    [SerializeField] float bubbleTime;
    [SerializeField] float bubbleEndScale;
    [SerializeField] float bubbleUpApmlitude;
    [SerializeField] Color bubbleColor;
    [Header("Crystal settings")]
    [SerializeField] GameObject XpCrystal;
    [SerializeField] float force;
    public float CrystalForce => force;
    [SerializeField] float timeToCollect;
    public float CrystalTimeToCollect => timeToCollect;
    [SerializeField] float collectSpeed;
    public float CrystalCollectSpeed => collectSpeed;
    Image me;
    Camera cam;
    bool declineXp;
    int[] xpTable;
    [Header("Sounds")]
    [SerializeField] float minPitch;
    [SerializeField] float pitchOctaveDownTime;
    [SerializeField] float semitonePerScore;
    AudioSource source;
    static bool delayedLevelUpEvent;
    float lastTimeCheating;
    void FillXpTable(int maxLevel)
    {
        xpTable = new int[maxLevel];
        for (int i = 0; i < maxLevel; i++)
        {
            xpTable[i] = PointsPerLevel * (i + 1);
            if (i > 0)
                xpTable[i] += xpTable[i - 1];
        }
    }
    int CalculateCurrentLevel()
    {
        for (int i = 0; i < xpTable.Length; i++)
        {
            if (xpTable[i] > Value)
                return i + 1;
        }
        return xpTable.Length + 1;
    }
    //public int GetLevel()
    //{
    //    var currentXp = Value;
    //    int currentLevel = 0;
    //    while (currentXp >= 0)
    //    {
    //        currentLevel++;
    //        currentXp -= PointsPerLevel * currentLevel;
    //    }
    //    return currentLevel;
    //}
    private void Start()
    {
        Inventory.InjectLevelUpEvent(this);
        if (GoalBase.Instance)
        {
            GoalBase.Instance.MissionCompleted += Hide;
            GoalBase.Instance.MissionFailed += Hide;
        }
        if (EndScreen.Instance)
            EndScreen.Instance.Resurrected += Show;

        if (delayedLevelUpEvent)
        {
            delayedLevelUpEvent = false;
            Curtain.Instance.OpeningCurtainDone += () => LevelUp?.Invoke(Level);
        }
    }
    public void SpawnXpCrystal(Vector3 particlePos, int count = 1)
    {
        if (declineXp)
            return;

        for (int i = 0; i < count; i++)
            PoolManager.Create(XpCrystal, particlePos, Quaternion.identity);
    }
    //public void AddXp(int baseValue, Vector3 particlePos)
    //{
    //    baseValue = AddXp(baseValue);
    //    switch (baseValue)
    //    {
    //        case 1:
    //            SpawnBubble(particlePos, bubbleColor, BubbleType.One);
    //            break;
    //        case 2:
    //            SpawnBubble(particlePos, bubbleColor, BubbleType.Two);
    //            break;
    //        case 5:
    //            SpawnBubble(particlePos, bubbleColor, BubbleType.Five);
    //            break;
    //        default:
    //            break;
    //    }
    //}
    public void SpawnBubble(Vector3 particlePos, Color color, BubbleType type)
    {
        particlePos.x = 0;
        particlePos += (cam.transform.position - particlePos).normalized * 2;
        var g = PoolManager.Create(bubblePrefab, particlePos);
        var bubbleText = g.GetComponentInChildren<SpriteRenderer>();
        bubbleText.sprite = bubbleSprites[(int)type];
        bubbleText.color = color;

        g.transform.localScale = Vector3.one;

        StartCoroutine(Bubbling(bubbleText, g.transform, particlePos));
    }
    IEnumerator Bubbling(SpriteRenderer bubble, Transform bubbleRoot, Vector3 startWorldPos)
    {
        float i = 0;
        var endWorldPos = startWorldPos + Vector3.up * bubbleUpApmlitude;
        var startColor = bubble.color;
        startColor.a = 1;
        var endColor = startColor;
        endColor.a = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / bubbleTime;
            bubbleRoot.position = Vector3.Lerp(startWorldPos, endWorldPos, i);
            bubbleRoot.localScale = Vector3.one * Mathf.Lerp(1, bubbleEndScale, i);
            bubble.color = Color.Lerp(startColor, endColor, i);
        }
        PoolManager.Erase(bubbleRoot.gameObject);
    }
    public void AddXp(int baseValue, bool silent = false)
    {
        source.pitch *= Mathf.Pow(1.0594630943592952645618252949463f, semitonePerScore);
        source.Play();
        baseValue *= Inventory.Instance.EffectCount(ItemEffect.DoubleXp) + 1;
        Value += baseValue;
        XpChanged?.Invoke(baseValue);
        if (Level <= xpTable.Length && Value >= xpTable[Level - 1])
        {
            Level++;
            if (GAManager.Instance)
                GAManager.Instance.SendEvent("Level_up_" + Level);
            if (!silent)
            {
                LevelUp?.Invoke(Level);
            }
            else
            {
                delayedLevelUpEvent = true;
            }
        }
        UpdateView();
    }
    void UpdateView()
    {
        //var valueNeedToLevelUp = (PointsPerLevel + currentLevel * PointsPerLevel) * currentLevel / 2;
        if (Level > xpTable.Length)
        {
            valueText.text = Value.ToString();
            fillImage.fillAmount = 1;
        }
        else
        {
            valueText.text = Value + "/" + xpTable[Level - 1];
            valueTextAnimationHelper.Pulse(true);
            if (Level < 2)
                fillImage.fillAmount = (float)Value / xpTable[Level - 1];
            else
                fillImage.fillAmount = (float)(Value - xpTable[Level - 2]) / (xpTable[Level - 1] - xpTable[Level - 2]);
        }
        me.sprite = levelSprites[Level - 1];
    }
    protected override void Save()
    {
        PlayerPrefs.SetInt("xp", Value);
    }
    protected override void Load()
    {
        valueTextAnimationHelper = valueText.GetComponent<UIAnimationHelper>();
        source = GetComponent<AudioSource>();
        me = GetComponent<Image>();
        cam = Camera.main;
        ///LOAD///
        Value = PlayerPrefs.GetInt("xp", 0);
#if DEBUG
        //Value = 0;
#endif
        FillXpTable(levelSprites.Length - 1);
        Level = CalculateCurrentLevel();
        ///UpdateView
        UpdateView();
    }
    public void Show()
    {
        declineXp = false;
        GetComponent<UIAnimationHelper>()?.Hide();
    }
    public void Hide()
    {
        declineXp = true;
        GetComponent<UIAnimationHelper>()?.Show();
    }
    private void Update()
    {
        source.pitch /= Mathf.Pow(2, Time.deltaTime / pitchOctaveDownTime);
        if (source.pitch < minPitch)
            source.pitch = minPitch;
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddXp(10);
            lastTimeCheating = Time.time;
        }
        if (Input.GetKey(KeyCode.X) && (Time.time - lastTimeCheating) > 0.5f)
        {
            AddXp(10);
        }
#endif
    }
}
