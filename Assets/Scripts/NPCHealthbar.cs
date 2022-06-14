using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCHealthbar : MonoBehaviour
{
    [SerializeField] Image portrait;
    [SerializeField] Image[] hearts;
    [SerializeField] Color fullHeart;
    [SerializeField] Color emptyHeart;
    [SerializeField] Image ghostHeart;
    [SerializeField] GameObject wantedNPCPrefab;
    UIAnimationHelper animationHelper;
    private void Awake()
    {
        animationHelper = GetComponent<UIAnimationHelper>();
        var color = ghostHeart.color;
        color.a = 0;
        ghostHeart.color = color;
        foreach (var i in hearts)
            i.color = fullHeart;
    }
    private void Start()
    {
        GoalBase.Instance.MissionFailed += Hide;
        GoalBase.Instance.MissionCompleted += Hide;
        EndScreen.Instance.Resurrected += Show;
        Show();
    }
    void Show()
    {
        animationHelper.Show();
    }
    void ApplyHealth(int value)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            var newColor = i < value ? fullHeart : emptyHeart;
            if (newColor != hearts[i].color && gameObject.activeInHierarchy)
            {
                StartCoroutine(FadingHeart(i, hearts[i].color, 1, 4));
            }
            hearts[i].color = newColor;
        }
    }
    IEnumerator FadingHeart(int heartIdx, Color startColor, float time, float scale)
    {
        var endColor = startColor;
        endColor.a = 0;
        ghostHeart.rectTransform.anchoredPosition = hearts[heartIdx].rectTransform.anchoredPosition;
        ghostHeart.color = startColor;
        ghostHeart.rectTransform.localScale = Vector3.one;
        float i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / time;
            ghostHeart.color = Color.Lerp(startColor, endColor, i);
            ghostHeart.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, scale, i);
        }
    }
    void Hide()
    {
        animationHelper.Hide();
    }
    public void Connect(NPC npc, GameObject prefab)
    {
        if (prefab != wantedNPCPrefab)
            return;

        portrait.sprite = npc.Portrait;
        ApplyHealth(npc.CurrentHealth);
        npc.Damaged += ApplyHealth;

        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].gameObject.SetActive(i < npc.MaxHealth);
            if (i == npc.MaxHealth - 1)
            {
                var rectTransform = GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(hearts[i].rectTransform.anchoredPosition.x + 72, rectTransform.rect.height);
            }
        }
    }
}
