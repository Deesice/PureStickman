using UnityEngine;
[CreateAssetMenu(menuName = "Custom/Quest", fileName = "NewQuest")]
public class Quest : ScriptableObject
{
    public GoalType missionType;
    public Sprite missonThumbnail;
    public bool IsCompleted => PlayerPrefs.GetInt("questCompleteMark: " + this.name, 0) > 0;
    public void CompleteQuest()
    {
        var cur = PlayerPrefs.GetInt("questCompleteMark: " + this.name, 0);
        PlayerPrefs.SetInt("questCompleteMark: " + this.name, cur + 1);
    }
}
