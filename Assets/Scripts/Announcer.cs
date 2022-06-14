using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Announcer : MonoBehaviour
{
    [SerializeField] Image backgroundImage;
    [SerializeField] Text titleText;
    [SerializeField] Outline titleOutline;
    [SerializeField] Text descText;
    [SerializeField] Color missionStartAnnounceColor;
    [SerializeField] Color levelUpAnnounceColor;
    [SerializeField] Color deathAnnounceColor;
    [SerializeField] Color missionCompleteColor;
    [SerializeField] Color extraLifeColor;
    [SerializeField] Color newItemColor;
    [Header("Announce parameters")]
    [SerializeField] Sound impactSound;
    [SerializeField] float splashTime;
    [SerializeField] float titleMaxScale;
    [SerializeField] float shakeRadius;
    [SerializeField] float flashTime;
    [SerializeField] float fadeInTime;
    [SerializeField] float delayTime;
    [SerializeField] float fadeOutTime;
    public static Announcer Instance { get; private set; }
    Color opacityColor;
    Vector3 titlePos;
    IEnumerator coroutine;
    CanvasScaler canvasScaler;
    System.Action previousCallback; 
    private void Awake()
    {
        canvasScaler = GetComponentInParent<CanvasScaler>();
        opacityColor = new Color(1, 1, 1, 0);
        Instance = this;
        ResetView();
    }
    void AnnounceMissionOrUnlockItem()
    {
        if (SceneManager.GetActiveScene().name != "Menu" && QuestView.currentQuest)
        {
            Announce(QuestView.LocaleMissionName,
                QuestView.LocaleMissionDescription,
                missionStartAnnounceColor);
        }
    }
    private void Start()
    {
        SmartInvoke.Invoke(AnnounceMissionOrUnlockItem, Curtain.DefaultTime, "", true);
        titlePos = titleText.rectTransform.position;

        Xp.Instance.LevelUp += (newLevel) => {
            Announce(LangAdapter.FindEntry("Gameplay", "LevelUp"),
                LangAdapter.FindEntry("Gameplay", "LevelUp_start")
                + newLevel
                + LangAdapter.FindEntry("Gameplay", "LevelUp_end"),
                levelUpAnnounceColor);
        };

        if (GoalBase.Instance)
        {
            GoalBase.Instance.MissionCompleted += () =>
            Announce(LangAdapter.FindEntry("Gameplay", "MissionComplete"), "", missionCompleteColor);

            GoalBase.Instance.MissionFailed += () =>
            Announce(LangAdapter.FindEntry("Gameplay", "Death"), "", deathAnnounceColor, EndScreen.Instance.ShowStats);
        }

        if (EndScreen.Instance)
        {
            EndScreen.Instance.Resurrected += () =>
            Announce(LangAdapter.FindEntry("Gameplay", "ExtraLife"), "", extraLifeColor);
        }
    }
    public void Announce(string titleEntry, string descEntry, Color color, System.Action callback = null)
    {
        ResetView();

        backgroundImage.color = color;
        titleText.text = titleEntry;
        descText.text = descEntry;

        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = Announcing(callback);
        previousCallback?.Invoke();
        previousCallback = callback;
        StartCoroutine(coroutine);
    }
    private void ResetView()
    {
        titleText.color = opacityColor;
        titleText.rectTransform.localScale = Vector3.one * titleMaxScale;
        titleOutline.effectColor = Color.black;
        descText.color = opacityColor;
        backgroundImage.fillAmount = 0;
    }
    public void PlayImpactSound()
    {
        impactSound.Play(GetComponent<AudioSource>());
    }
    IEnumerator Announcing(System.Action callback)
    {
        PlayImpactSound();
        float i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / splashTime;
            backgroundImage.fillAmount = i;
            titleText.color = Color.Lerp(opacityColor, Color.white, i);
            titleText.rectTransform.localScale = Vector3.one * Mathf.Lerp(titleMaxScale, 1, i);
        }
        i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / flashTime;
            var sin = Mathf.Sin(i * Mathf.PI * 2);
            titleOutline.effectColor = Color.Lerp(Color.black, Color.white, sin);
            var random = Random.Range(0, Mathf.PI * 2);
            titleText.rectTransform.position = titlePos
                + new Vector3(Mathf.Cos(random), Mathf.Sin(random), 0)
                * Mathf.Lerp(0, shakeRadius * canvasScaler.GetRelative4K(), sin);
        }
        previousCallback = null;
        callback?.Invoke();
        i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / fadeInTime;
            descText.color = Color.Lerp(opacityColor, Color.white, i);
        }
        i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / delayTime;
        }
        i = 0;
        var initialColor = backgroundImage.color;
        var endColor = backgroundImage.color;
        endColor.a = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / fadeOutTime;
            descText.color = Color.Lerp(Color.white, opacityColor, i);
            titleText.color = Color.Lerp(Color.white, opacityColor, i);
            backgroundImage.color = Color.Lerp(initialColor, endColor, i);
        }
        coroutine = null;
    }
}
