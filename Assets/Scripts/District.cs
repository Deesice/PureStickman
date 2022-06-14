using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class District : MonoBehaviour, IValidate
{
    public static int TotalDistrictCount { get; private set; }
    public static int SelectedDistrictLevel { get; private set; }
    public static float DifficultyGradient => TotalDistrictCount > 1 ? (float)(SelectedDistrictLevel - 1) / (TotalDistrictCount - 1) : -1;
    [SerializeField] int districtLevel;
    QuestMarker[] quests;
    [SerializeField] Collider districtCollider;
    [SerializeField] string _sceneName;
    [SerializeField] AOUAD.Reward _reward;
    public static AOUAD.Reward CurrentReward { get; private set; }
    public static string SceneName { get; private set; }
    bool opened;
    bool openFrameAgo;
    Camera cam;
    [Header("District outlint parameters")]
    [SerializeField] Material lockedMaterialOutline;
    [SerializeField] Material unlockedMaterialOutline;
    [SerializeField] Material lockedMaterialInner;
    [SerializeField] Material unlockedMaterialInner;
    [SerializeField] Renderer outline;
    [SerializeField] Renderer inner;
    [SerializeField] Text districtNameText;
    [SerializeField] Text districtLevelText;
    Vector3 initialInnerScale;
    Vector3 initialRootPosition;
    [SerializeField] UnityEvent OnEnter;
    List<QuestMarker> ignoredMarkers = new List<QuestMarker>();
    [SerializeField] UnityEvent OnOpeningQuest;
    private void Awake()
    {
        quests = GetComponentsInChildren<QuestMarker>(true);
        initialRootPosition = transform.position;
        if (TotalDistrictCount == 0)
        {
            //Вычитаем меню
            TotalDistrictCount = SceneManager.sceneCountInBuildSettings - 1;
        }
        cam = Camera.main;
        initialInnerScale = inner.transform.localScale;
        Close();
    }
    void Start()
    { 
        if (IsValid())
        {
            districtNameText.color = Color.white;
            districtLevelText.color = Color.white;
            outline.sharedMaterial = unlockedMaterialOutline;
            inner.sharedMaterial = unlockedMaterialInner;
        }
        else
        {
            districtNameText.color = Color.red;
            districtLevelText.color = Color.red;
            outline.sharedMaterial = lockedMaterialOutline;
            inner.sharedMaterial = lockedMaterialInner;
        }
        LangAdapter.instance.OnLanguageChanged += SetupText;
        SetupText();
        QuestView.Instance.AnimationHelper.StateAnimationEnded += OnQuestViewStateAnimationEnded;
    }
    void OnQuestViewStateAnimationEnded(bool questIsShowingNow)
    {
        if (questIsShowingNow && opened)
            OnOpeningQuest.Invoke();
    }
    void SetupText()
    {
        districtLevelText.text = LangAdapter.FindEntry("Districts", "DistrictNotEnoughLevel") + districtLevel;
    }
    static bool FadingMaterialStep(Material material, float targetAlphaValue, float speed)
    {
        var color = material.color;
        var m = Mathf.Abs(color.a - targetAlphaValue);
        if (m > 0)
        {
            color.a = Mathf.Lerp(color.a, targetAlphaValue, Time.deltaTime * speed / m);
            material.color = color;
            return true;
        }
        return false;
    }
    IEnumerator FadingOutlineAndInner(bool hide, float speed)
    {
        float maxScale = 2;
        bool flag = true;
        float targetScale = hide ? maxScale : 1;
        float currentScale = hide ? 1 : maxScale;
        while (flag)
        {
            yield return null;
            flag = false;
            flag |= FadingMaterialStep(unlockedMaterialInner, hide ? 0 : 0.125f, speed * 0.125f);
            flag |= FadingMaterialStep(lockedMaterialInner, hide ? 0 : 0.125f, speed * 0.125f);

            flag |= FadingMaterialStep(unlockedMaterialOutline, hide ? 0 : 1, speed);
            flag |= FadingMaterialStep(lockedMaterialOutline, hide ? 0 : 1, speed);

            var m = Mathf.Abs(currentScale - targetScale);
            if (m > 0)
            {
                flag = true;
                currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * speed * (maxScale - 1) / m);
                inner.transform.localScale = currentScale * initialInnerScale;
            }

            m = Mathf.Abs(districtNameText.color.a - (hide ? 0 : 1));
            if (m > 0)
            {
                flag = true;
                var color = districtNameText.color;
                color.a = Mathf.Lerp(color.a, hide ? 0 : 1, Time.deltaTime * speed / m);
                districtNameText.color = color;
                districtLevelText.color = color;
            }
        }
    }
    public void Open()
    {
        CurrentReward = _reward;
        SceneName = _sceneName;
        SelectedDistrictLevel = districtLevel;
        opened = true;
        openFrameAgo = true;
        districtCollider.enabled = false;
        foreach (var q in quests)
            q.gameObject.SetActive(true);

        StartCoroutine(FadingOutlineAndInner(true, 4));
        OnEnter.Invoke();
    }
    public void Close()
    {
        opened = false;
        districtCollider.enabled = true;
        foreach (var q in quests)
            q.gameObject.SetActive(false);

        StartCoroutine(FadingOutlineAndInner(false, 4));
    }
    private void Update()
    {
        if (!opened || QuestView.Instance.Showing)
            return;

        if (openFrameAgo)
        {
            openFrameAgo = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hit, 10000, -1, QueryTriggerInteraction.Collide))
            {
                var q = hit.collider.GetComponentInParent<QuestMarker>();
                if (!q)
                {
                    var l = hit.collider.GetComponentInParent<QuestMarkerLight>();
                    if (l)
                    {
                        q = l.host;
                    }
                }
                if (q && !ignoredMarkers.Contains(q))
                {
                    OpenQuestDescription(q);
                }
            }
        }
    }
    void OpenQuestDescription(QuestMarker quest)
    {
        QuestView.Instance.Show();
        QuestView.Instance.DisplayQuest(quest);
    }
    public void CloseQuestDescription()
    {
        QuestView.Instance.Hide();
    }

    public bool IsValid()
    {
        return Xp.Instance.Level >= districtLevel;
    }
    public void Shake()
    {
        StartCoroutine(Shaking(0.25f, 0.25f));
    }
    IEnumerator Shaking(float time, float maxAmplitude)
    {
        float i = 0;
        while (i < 1)
        {
            yield return null;
            var amplitude = Mathf.Sin(i * Mathf.PI) * maxAmplitude;
            var angleRad = Random.Range(0, Mathf.PI * 2);
            var v = cam.transform.right * Mathf.Cos(angleRad) + cam.transform.up * Mathf.Sin(angleRad);
            v.Normalize();
            transform.position = initialRootPosition + v * amplitude;
            i += Time.deltaTime / time;
        }

        transform.position = initialRootPosition;
    }
    public void ConvertQuests()
    {
        if (districtLevel < Xp.Instance.Level)
            foreach (var q in quests)
                if (q.quest)
                    q.quest.CompleteQuest();
    }
    public void IgnoreAllMarkers()
    {
        ignoredMarkers.AddRange(transform.GetComponentsInChildren<QuestMarker>(true));
    }
    public void ClearIgnoreList(QuestMarker d)
    {
        ignoredMarkers.Remove(d);
    }
    public void ClearIgnoreList()
    {
        ignoredMarkers.Clear();
    }
}
