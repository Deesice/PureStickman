using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Tutorial : SaveLoadBehaviour<Tutorial>
{
    [SerializeField] Item firstBuyingItem;
    public static bool IsShowedNow => showingState >= 0;
    static int showingState = -1;
    List<Tuple<Transform, int>> backupHighlightInfo = new List<Tuple<Transform, int>>();
    [SerializeField] float fadingSpeed;
    [Range(0, 1)]
    [SerializeField] float backgroundAlpha;
    [SerializeField] TuturialState[] states;
    [SerializeField] int lastShowedHints = -1;
    Image background;
    readonly Queue<int> hintQueue = new Queue<int>();
    bool IsMenu => SceneManager.GetActiveScene().name == "Menu";
    protected override void Save()
    {
        PlayerPrefs.SetInt("lastShowedHint" + IsMenu, lastShowedHints);
    }
    protected override void Load()
    {
        foreach (var i in states)
        {
            i.tutorialRoots.SetActive(false);
        }
        background = GetComponent<Image>();
        background.enabled = true;
        var color = background.color;
        color.a = 0;
        background.color = color;
        foreach (var i in states)
        {
            foreach (var m in i.tutorialRoots.GetComponentsInChildren<MaskableGraphic>())
            {
                color = m.color;
                color.a = 0;
                m.color = color;
            }
        }
        ///LOAD///
        lastShowedHints = PlayerPrefs.GetInt("lastShowedHint" + IsMenu, -1);
    }
    void Start()
    {
        bool output = false;
        if (Xp.Instance.Value == 0)
        {
            ShowState(0, out output);
        }
        if (!output && Inventory.Instance.GetShardsCount(GoalType.Kills) >= 16)
        {
            if (Inventory.Instance.GetProgress(firstBuyingItem) > 0)
                lastShowedHints = 8;
            else
                ShowState(6, out output);
        }
        gameObject.SetActive(output);
    }
    IEnumerator Fading(MaskableGraphic[] maskableGraphics, float targetAlpha, Action callback = null)
    {
        bool flag = true;
        Color color;
        float m;
        while (flag)
        {
            flag = false;
            foreach(var g in maskableGraphics)
            {
                color = g.color;
                m = Mathf.Abs(targetAlpha - color.a);
                if (m > 0)
                {
                    color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * fadingSpeed / m);
                    g.color = color;
                    flag = true;
                }
            }
            color = background.color;
            m = Mathf.Abs(targetAlpha * backgroundAlpha - color.a);
            if (m > 0)
            {
                color.a = Mathf.Lerp(color.a, targetAlpha * backgroundAlpha, Time.deltaTime * fadingSpeed * backgroundAlpha / m);
                background.color = color;
                flag = true;
            }
            yield return null;
        }
        callback?.Invoke();
    }
    public void ShowState(int state)
    {
        ShowState(state, out _);
    }
    public void ShowState(int state, out bool output)
    {
        if (lastShowedHints + 1 != state)
        {
            output = false;
            return;
        }

        if (showingState >= 0)
        {
            hintQueue.Enqueue(state);
            output = false;
            return;
        }

        showingState = state;
        var currentState = states[state];
        currentState.tutorialRoots.SetActive(true);
        foreach (var mg in currentState.highlightedGraphic)
        {
            backupHighlightInfo.Add(new Tuple<Transform, int>(
                mg.parent, mg.GetSiblingIndex()));
            mg.parent = transform.parent;
            mg.SetAsLastSibling();
        }
        gameObject.SetActive(true);
        background.sprite = currentState.customMask;
        StartCoroutine(Fading(currentState.tutorialRoots.GetComponentsInChildren<MaskableGraphic>(), 1));
        output = true;
        currentState.OnEnter.Invoke();
        if (currentState.showTime > 0)
        {
            SmartInvoke.Invoke(HideCurrentState, currentState.showTime, "", true);
        }
    }
    public void HideState(int state)
    {
        if (showingState != state)
            return;

        HideCurrentState();
    }
    [ContextMenu("Hide current state")]
    public void HideCurrentState()
    {
        if (showingState < 0)
            return;

        if (lastShowedHints == showingState)
            return;

        StopAllCoroutines();
        var currentState = states[showingState];
        lastShowedHints = showingState;
        StartCoroutine(Fading(currentState.tutorialRoots.GetComponentsInChildren<MaskableGraphic>(), 0, NextHintInQueue));
        var i = 0;
        foreach (var mg in currentState.highlightedGraphic)
        {
            mg.parent = backupHighlightInfo[i].Item1;
            mg.SetSiblingIndex(backupHighlightInfo[i].Item2);
        }
        backupHighlightInfo.Clear();
        currentState.OnExit.Invoke();
    }
    void NextHintInQueue()
    {
        states[showingState].tutorialRoots.SetActive(false);
        showingState = -1;
        gameObject.SetActive(false);
        if (hintQueue.Count > 0)
        {
            ShowState(hintQueue.Dequeue());
        }
    }
}
[Serializable]
class TuturialState
{
    public RectTransform[] highlightedGraphic;
    public GameObject tutorialRoots;
    public UnityEvent OnEnter;
    public UnityEvent OnExit;
    public Sprite customMask;
    public float showTime;
}
