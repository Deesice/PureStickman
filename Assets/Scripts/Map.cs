using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] float curtainSwitchHalfTime;
    [SerializeField] Transform mapCameraPos;
    [SerializeField] Transform defaultCameraPos;
    [SerializeField] QuestView questView;
    [SerializeField] GameObject[] OnDiscoverMapElements;
    [SerializeField] GameObject[] OnCharacterLibraryElements;
    [SerializeField] GameObject[] OnFocusOnDistrictElements;
    [SerializeField] Color menuLightColor;
    [SerializeField] Color mapLightColor;

    bool discoverMapNow;
    Camera cam;
    new Light light;
    Vector3 wantedPos;
    public District CapturedDistrict { get; private set; }
    float initialOrthographicSize;
    float wantedOrthographicSize;
    List<District> ignoredDistricts = new List<District>();
    private void Awake()
    {
        wantedPos = defaultCameraPos.position;
        light = FindObjectOfType<Light>();
        cam = Camera.main;
        initialOrthographicSize = cam.orthographicSize;
        wantedOrthographicSize = initialOrthographicSize;

        foreach (var i in OnFocusOnDistrictElements)
            i.SetActive(false);
        foreach (var i in OnDiscoverMapElements)
            i.SetActive(false);
        foreach (var i in OnCharacterLibraryElements)
            i.SetActive(true);
    }
    [ContextMenu("Switch map")]
    public void SwitchMap()
    {
        //if (Xp.Instance.Value > 0)
        //{
            Curtain.Instance.SetColor(Color.white);
            Curtain.Instance.Close(curtainSwitchHalfTime, OnCurtainSwitched);
            //StartCoroutine(ChangingNearPlane(18, curtainSwitchHalfTime));
        //}
        //else
        //{
        //    QuestView.LoadScene("Tutorial", "");
        //}
    }
    void OnCurtainSwitched()
    {
        discoverMapNow = !discoverMapNow;
        
        var wantedAnchor = discoverMapNow ? mapCameraPos : defaultCameraPos;
        cam.transform.position = wantedAnchor.position;
        cam.transform.rotation = wantedAnchor.rotation;
        cam.orthographic = discoverMapNow;
        wantedPos = wantedAnchor.position;

        light.color = discoverMapNow ? mapLightColor : menuLightColor;

        foreach (var g in OnDiscoverMapElements)
            g.SetActive(discoverMapNow);
        foreach (var g in OnCharacterLibraryElements)
            g.SetActive(!discoverMapNow);

        Curtain.Instance.Open(curtainSwitchHalfTime);
        //StartCoroutine(ChangingNearPlane(2, curtainSwitchHalfTime));
    }
    IEnumerator ChangingNearPlane(float targetValue, float time)
    {
        float currentValue = cam.nearClipPlane;
        float i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / time;
            cam.nearClipPlane = Mathf.Lerp(currentValue, targetValue, i);
        }
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (!CapturedDistrict)
            wantedPos = (discoverMapNow ? mapCameraPos : defaultCameraPos).position;
#endif
        cam.transform.position = Vector3.Lerp(cam.transform.position, wantedPos, Time.deltaTime * 10);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, wantedOrthographicSize, Time.deltaTime * 10);

        if (questView.Showing || CapturedDistrict)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hit, 10000, -1, QueryTriggerInteraction.Collide))
            {
                var q = hit.collider.GetComponentInParent<District>();
                if (q && !ignoredDistricts.Contains(q))
                {
                    if (q.IsValid())
                    {
                        wantedPos = cam.transform.position
                            + Vector3.ProjectOnPlane(hit.collider.bounds.center - cam.transform.position,
                                cam.transform.forward);
                        wantedOrthographicSize = 5; //Vector3.Dot(hit.collider.bounds.extents, cam.transform.up) / Mathf.Sqrt(2);
                        CapturedDistrict = q;
                        OpenDistrict();
                    }
                    else
                    {
                        UIButton.PlayErrorSound();
                        q.Shake();
                    }
                }
            }
        }
    }
    void OpenDistrict()
    {
        UIButton.PlayButtonSound();
        CapturedDistrict.Open();
        foreach (var i in OnFocusOnDistrictElements)
            i.SetActive(true);
        foreach (var i in OnDiscoverMapElements)
            i.SetActive(false);
    }
    public void CloseDistrict()
    {
        wantedOrthographicSize = initialOrthographicSize;
        wantedPos = mapCameraPos.position;
        CapturedDistrict.Close();
        CapturedDistrict = null;

        foreach (var i in OnFocusOnDistrictElements)
            i.SetActive(false);
        foreach (var i in OnDiscoverMapElements)
            i.SetActive(true);
    }
    public void IgnoreAllDistricts()
    {
        ignoredDistricts.AddRange(FindObjectsOfType<District>(true));
    }
    public void ClearIgnoreList(District d)
    {
        ignoredDistricts.Remove(d);
    }
    public void ClearIgnoreList()
    {
        ignoredDistricts.Clear();
    }
}
