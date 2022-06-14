using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIAnimationHelper : MonoBehaviour, IValidate
{
    [SerializeField] float translateSpeed;
    [SerializeField] float showSpeed = 10;
    [SerializeField] float fillSpeed;
    [SerializeField] AnimationCurve pulseCurve;
    [SerializeField] float pulseDuration;
    new RectTransform transform;
    Image image;
    public event Action<float> FillEvent;
    [SerializeField] UnityEvent OnFillingComplete;
    [SerializeField] UnityEvent OnTranslatingComplete;
    [SerializeField] RectTransform showPos;
    [SerializeField] RectTransform hidePos;
    public bool Showing { get; private set; }
    bool hideAndShowOption;
    public Action<bool> StateAnimationEnded;
    Vector3 wantedPosition
    {
        get
        {
            if (Showing)
            {
                if (useAnchoredPosition)
                    return showPos.anchoredPosition3D;
                else
                    return showPos.position;
            }
            else
            {
                if (useAnchoredPosition)
                    return hidePos.anchoredPosition3D;
                else
                    return hidePos.position;
            }
        }
    }
    public Vector3 ShowPosition => showPos.position;
    [SerializeField] bool useAnchoredPosition;
    private void Awake()
    {
        hideAndShowOption = showPos && hidePos;
        image = GetComponent<Image>();
        transform = GetComponent<RectTransform>();
    }
    public void Pulse(bool growing)
    {
        StartCoroutine(Pulsing(growing));
    }
    public void Show()
    {
        Showing = true;
    }
    public void Hide()
    {
        Showing = false;
    }
    IEnumerator Pulsing(bool growing)
    {
        float i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / pulseDuration;
            transform.localScale = Vector3.one * (1 + (growing ? pulseCurve.Evaluate(i) : -pulseCurve.Evaluate(i)));
        }
    }
    void Update()
    {
        if (hideAndShowOption)
        {
            var currentPosition = useAnchoredPosition ? transform.anchoredPosition3D : transform.position;
            //var m = (wantedPosition - currentPosition).sqrMagnitude;
            if ((wantedPosition - currentPosition).sqrMagnitude > 0.1f)
            {
                currentPosition = Vector3.Lerp(currentPosition, wantedPosition, Time.deltaTime * showSpeed);
                if (useAnchoredPosition)
                    transform.anchoredPosition3D = currentPosition;
                else
                    transform.position = currentPosition;

                if ((wantedPosition - currentPosition).sqrMagnitude <= 0.1f)
                    StateAnimationEnded?.Invoke(Showing);
            }
        }
    }
    public void Translate(RectTransform newPos)
    {
        StartCoroutine(Translating(newPos));
    }
    IEnumerator Translating(RectTransform newPos)
    {
        var magnitude = (transform.position - newPos.position).magnitude;
        while (magnitude > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, newPos.position, Time.deltaTime * translateSpeed / magnitude);
            magnitude = (transform.position - newPos.position).magnitude;
            yield return null;
        }
        transform.position = newPos.position;
        OnTranslatingComplete.Invoke();
    }
    public void Fill(float value)
    {
        StartCoroutine(Filling(Mathf.Clamp01(value)));
    }
    IEnumerator Filling(float targetValue)
    {
        while (image.fillAmount != targetValue)
        {
            yield return null;
            image.fillAmount = Mathf.Lerp(image.fillAmount,
                targetValue,
                Time.deltaTime * fillSpeed / Mathf.Abs(image.fillAmount - targetValue));
        }
        if (targetValue == 1)
            OnFillingComplete.Invoke();
        FillEvent?.Invoke(targetValue);
    }

    public bool IsValid()
    {
        return !Showing;
    }
}
