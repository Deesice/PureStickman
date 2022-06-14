using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartInvoke : MonoBehaviour
{
    class Info
    {
        public string key;
        public Action action;
        public float time;
        public bool scaled;
        public void Release(float scaledTime, float unscaledTime)
        {
            time -= scaled ? scaledTime : unscaledTime;
            if (time <= 0)
            {
                SmartInvoke.Unsubscribe(this);
                var a = action;
                action = null;
                a.Invoke();
            }
        }
    }
    public delegate bool MyPredicate();
    static SmartInvoke _instance;
    List<Info> delegates = new List<Info>();
    static SmartInvoke instance
    {
        get {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<SmartInvoke>();
            return _instance; }
    }
    static bool destroyed;
    event Action<float, float> update;
    private void Awake()
    {
        destroyed = false;
    }
    private void Update()
    {
        update?.Invoke(Time.deltaTime, Time.unscaledDeltaTime);
    }
    static void Unsubscribe(Info i)
    {
        instance.update -= i.Release;
    }
    public static void Invoke(Action action, float time, string tag = "", bool scaledTime = false)
    {
        if (destroyed)
            return;

        var i = new Info
        {
            time = time,
            action = action,
            key = tag,
            scaled = scaledTime
        };
        instance.delegates.Add(i);
        instance.update += i.Release;
    }
    public static new void CancelInvoke(string tag)
    {
        if (destroyed)
            return;

        foreach (var i in instance.delegates)
        {
            if (i.key == tag)
                instance.update -= i.Release;
        }
    }
    public static bool ResumeInvoke(string target)
    {
        bool b = false;
        foreach (var i in instance.delegates.FindAll((t) => t.key == target))
        {
            instance.update -= i.Release;
            var a = i.action;
            b |= i.action != null;
            i.action = null;
            a?.Invoke();
        }
        return b;
    }
    public static void WhenTrue(MyPredicate p, Action a)
    {
        instance.StartCoroutine(instance.WhenTrueCoroutine(p, a));
    }
    IEnumerator WhenTrueCoroutine(MyPredicate p, Action a)
    {
        while (!p.Invoke())
            yield return null;
        a.Invoke();
    }
    private void OnDestroy()
    {
        destroyed = true;
        StopAllCoroutines();
    }
}
