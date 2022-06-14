using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
public interface IValidate
{
    bool IsValid();
}

public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public UnityEvent actions;
    Shadow shadow;
    new RectTransform transform;
    MaskableGraphic background;
    Color color;
    bool _locked;
    bool Locked { get { return _locked; } set {
            if (_locked != value)
            {
                _locked = value;
                background.color = (value ? Color.grey : color);
            }
        } }
    [SerializeField] Component[] validators;
    List<IValidate> actualValidators = new List<IValidate>();
    CanvasScaler canvasScaler;
    static AudioSource source;
    static Sound clickSound;
    static Sound errorSound;
    static float lastErrorSoundTime;
    private void Update()
    {
        if (actualValidators.Count > 0)
        {
            foreach (var v in actualValidators)
                if (!v.IsValid())
                {
                    Locked = true;
                    return;
                }
            Locked = false;
        }
    }
    private void Awake()
    {
        if (!source)
        {
            source = new GameObject("button_audiosource").AddComponent<AudioSource>();
            clickSound = Resources.Load<Sound>("button_sound");
            errorSound = Resources.Load<Sound>("error_sound");
        }
        canvasScaler = GetComponentInParent<CanvasScaler>();
        transform = GetComponent<RectTransform>();
        foreach (var s in GetComponents<Shadow>())
            if (!(s is Outline))
                shadow = s;
        background = GetComponent<MaskableGraphic>();
        color = background.color;
        color.a = 1;

        if (validators == null)
            return;

        foreach (var v in validators)
            if (v is IValidate)
                actualValidators.Add(v as IValidate);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (Locked)
            return;

        if (shadow.enabled)
        {
            shadow.enabled = false;
            transform.localPosition += new Vector3(shadow.effectDistance.x, shadow.effectDistance.y, 0);
            background.color = new Color(color.r / 2, color.g / 2, color.b / 2, color.a);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!shadow.enabled)
        {
            shadow.enabled = true;
            transform.localPosition += new Vector3(-shadow.effectDistance.x, -shadow.effectDistance.y, 0);
            background.color = color;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!Locked && transform.Contains(eventData.position, canvasScaler))
        {
            OnPointerExit(null);
            actions.Invoke();
            PlayButtonSound();
        }
    }
    public static void PlayButtonSound()
    {
        clickSound.Play(source);
    }
    public static void PlayErrorSound()
    {
        errorSound.Play(source);
        lastErrorSoundTime = Time.unscaledTime;
    }
    public void ShowInterstitial()
    {
        InterAd.ShowAd(GetComponentInParent<Canvas>());
    }
    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }
}