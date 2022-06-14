using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestView : MonoBehaviour
{
    [SerializeField] Image[] difficultyMarkers;
    [SerializeField] Text xpText;
    [SerializeField] Image xpImage;
    [SerializeField] Text moneyText;
    [SerializeField] Image[] moneyImages;
    [SerializeField] Text missionName;
    [SerializeField] Text missionDescription;
    [SerializeField] Image locationPreview;
    [SerializeField] Color curtainLoadColor;
    [SerializeField] Color difficultyMarkerColor;
    public static Quest currentQuest { get; private set; }
    public UIAnimationHelper AnimationHelper { get; private set; }
    public bool Showing => AnimationHelper.Showing;
    public static QuestView Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        AnimationHelper = GetComponent<UIAnimationHelper>();
    }
    public static string LocaleMissionName => LangAdapter.FindEntry("Districts", District.SceneName) + ": " + LangAdapter.FindEntry("Missions", ((int)currentQuest.missionType).ToString());
    public static string LocaleMissionDescription => LangAdapter.FindEntry("Missions", ((int)currentQuest.missionType).ToString() + "_desc");
    public void DisplayQuest(QuestMarker quest)
    {
        UIButton.PlayButtonSound();
        currentQuest = quest.quest;
        missionName.text = LocaleMissionName;
        missionDescription.text = LocaleMissionDescription;
        locationPreview.sprite = quest.quest.missonThumbnail;

        xpImage.enabled = !currentQuest.IsCompleted;
        xpText.enabled = !currentQuest.IsCompleted;
        xpText.text = "+ " + District.CurrentReward.xp;

        foreach (var i in moneyImages)
            i.enabled = false;

        moneyImages[(int)Inventory.Instance.ConvertToUnlockedCurrency(currentQuest.missionType)].enabled = true;
        moneyText.text = "+ " + District.CurrentReward.money;

        SetupDifficulty(District.SelectedDistrictLevel);
    }
    void SetupDifficulty(int difficulty)
    {
        if (difficulty > difficultyMarkers.Length)
            difficulty = difficultyMarkers.Length;

        int i = 0;
        for (; i < difficulty; i++)
        {
            difficultyMarkers[i].enabled = true;
            difficultyMarkers[i].color = difficultyMarkerColor;
        }
        for (; i < Mathf.Min(difficultyMarkers.Length, District.TotalDistrictCount); i++)
        {
            difficultyMarkers[i].enabled = true;
            difficultyMarkers[i].color = Color.gray;
        }
        for (; i < difficultyMarkers.Length; i++)
        {
            difficultyMarkers[i].enabled = false;
        }
    }
    public void Show()
    {
        AnimationHelper.Show();
    }
    public void Hide()
    {
        AnimationHelper.Hide();
    }
    public void LoadCurrentQuest()
    {
        LoadScene(District.SceneName, currentQuest.missionType.ToString());
    }
    public static void LoadScene(string sceneName, string missionName)
    {
        Instance.AnimationHelper.Hide();
        Curtain.Instance.SetColor(Instance.curtainLoadColor);
        Curtain.Instance.Close(1, () => 
        {
            GAManager.Instance.SendEventOnce(sceneName + "_" + missionName);
            SceneManager.LoadScene(sceneName);
        });
    }
}
