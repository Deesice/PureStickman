using System;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Canvas))]
public class Compass : MonoBehaviour
{
    [SerializeField] CompassView prefab;
    [Range(0, 0.05f)]
    [SerializeField] float amplitude;
    [SerializeField] float speed;
    [SerializeField] CompassParameters[] parameters;
    static Compass instance;
    Camera cam;
    Dictionary<Transform, int> compassRequests = new Dictionary<Transform, int>();
    List<Vector2>[] viewportPoses;
    List<CompassView> compassViews = new List<CompassView>();
    private void Awake()
    {
        viewportPoses = new List<Vector2>[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            viewportPoses[i] = new List<Vector2>();
        }
        cam = Camera.main;
        instance = this;
        //cam.GetComponent<CameraBehaviour>().Updated += OnCameraUpdated;
    }
    public static void RemoveCompass(Transform target)
    {
        instance.compassRequests.Remove(target);
    }
    public static void AddCompass(Transform target, CompassType type)
    {
        instance.compassRequests.Remove(target);
        instance.compassRequests.Add(target, (int)type);
    }
    void SyncViewports()
    {
        int i = 0;
        for (int compassIdx = 0; compassIdx < viewportPoses.Length; compassIdx++)
        {
            foreach (var pos in viewportPoses[compassIdx])
            {
                CompassView v;
                if (i >= compassViews.Count)
                {
                    v = PoolManager.Create(prefab.gameObject, Vector3.zero).GetComponent<CompassView>();
                    v.transform.SetParent(transform, false);
                    v.transform.SetAsFirstSibling();
                    compassViews.Add(v);
                }
                else
                {
                    v = compassViews[i];
                }
                if (compassIdx != v.compassTypeIdx)
                {
                    var currentParams = parameters[compassIdx];
                    v.Setup(LangAdapter.FindEntry(currentParams.textCategory, currentParams.textEntry), currentParams.color, compassIdx);
                }
                v.rectTransform.anchorMin = pos;
                v.rectTransform.anchorMax = pos;
                v.rectTransform.anchoredPosition = Vector2.zero;
                v.SetPivot(pos.x);
                i++;
            }
        }
        for (; i < compassViews.Count; i++)
        {
            var v = compassViews[i];
            if (v.rectTransform.anchorMin.x >= 0)
            {
                v.rectTransform.anchorMin = -Vector2.one;
                v.rectTransform.anchorMax = -Vector2.one;
                v.rectTransform.anchoredPosition = Vector2.zero;
            }
        }
    }
    void OnGUI()
    {
        foreach (var i in viewportPoses)
            i.Clear();

        var sinOffset = Mathf.Sin(Time.time * speed) * amplitude;
        Vector2 leftAttackMax;
        leftAttackMax.x = -1;
        leftAttackMax.y = -1;
        Vector2 rightAttackMin;
        rightAttackMin.x = 2;
        rightAttackMin.y = 2;

        foreach (var pair in compassRequests)
        {
            var viewport = cam.CustomWorldToViewportPoint(pair.Key.position + parameters[pair.Value].offset);
            //viewport.z = Mathf.Abs(viewport.z);
            viewport.x = Mathf.Clamp01(viewport.x);
            switch(pair.Value)
            {
                case 0:
                    if (viewport.x < 0.5f && viewport.x > leftAttackMax.x)
                    {
                        leftAttackMax.x = viewport.x;
                        leftAttackMax.y = Mathf.Clamp01(viewport.y + sinOffset);
                        continue;
                    }
                    if (viewport.x > 0.5f && viewport.x < rightAttackMin.x)
                    {
                        rightAttackMin.x = viewport.x;
                        rightAttackMin.y = Mathf.Clamp01(viewport.y + sinOffset);
                        continue;
                    }
                    continue;
                default:
                    break;
            }
            viewport.y = Mathf.Clamp01(viewport.y + sinOffset);
            viewportPoses[pair.Value].Add(viewport);
        }
        if (DifficultyManager.GetGoalType() == GoalType.Kills)
        {
            viewportPoses[0].Add(leftAttackMax);
            viewportPoses[0].Add(rightAttackMin);
        }
        SyncViewports();
    }
}
public enum CompassType { Attack, Stay, Defend, Run }
[Serializable]
struct CompassParameters
{
    public string textCategory;
    public string textEntry;
    public Color color;
    public Vector3 offset;
}
