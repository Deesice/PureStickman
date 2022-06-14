using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Wallet : MonoBehaviour
{
    [SerializeField] Text text;
    [SerializeField] GoalType valueType;
    [SerializeField] float shakeAmplitude;
    Vector3 initialTextPosition;
    private void Start()
    {
        UpdateView(valueType, Inventory.Instance.GetShardsCount(valueType));
        Inventory.Instance.ShardsCountChanged += UpdateView;
        Inventory.Instance.NotEnoughMoneyWarning += PlayWarning;
        initialTextPosition = text.rectTransform.position;
        gameObject.SetActive(Inventory.Instance.IsMoneyTypeUnlocked(valueType));
    }
    void PlayWarning(GoalType s)
    {
        if (s != valueType)
            return;

        StartCoroutine(Shaking(0.25f));
    }
    IEnumerator Shaking(float time)
    {
        float i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / time;
            text.rectTransform.position = initialTextPosition + (Random.Range(0, 2) * 2 - 1) * Mathf.Sin(i * Mathf.PI) * shakeAmplitude * Vector3.right;
            text.color = Color.Lerp(Color.black, Color.red, Mathf.Sin(i * Mathf.PI));
        }
        text.rectTransform.position = initialTextPosition;
    }
    void UpdateView(GoalType s, int value)
    {
        if (s == valueType)
            text.text = value.ToString();
    }
    private void OnDestroy()
    {
        Inventory.Instance.ShardsCountChanged -= UpdateView;
        Inventory.Instance.NotEnoughMoneyWarning -= PlayWarning;
    }
}
