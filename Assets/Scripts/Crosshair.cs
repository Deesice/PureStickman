using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    //[SerializeField] RectTransform fixPoint;
    [Range(0, 1)]
    [SerializeField] float minTenseStateToShoot;
    [SerializeField] Image outline;
    [SerializeField] Image mainDot;
    [SerializeField] float tensedScale;
    [SerializeField] float tensedSpeed;
    [ColorUsageAttribute(false)]
    [SerializeField] Color tensedColor;
    [SerializeField] float relaxedScale;
    [SerializeField] float relaxedSpeed;
    [ColorUsageAttribute(false)]
    [SerializeField] Color relaxedColor;
    [SerializeField] float sensitivity;
    [SerializeField] float mouseSensitivityMultiplier = 1;
    [Header("Layout")]
    [SerializeField] float dotSpeed;
    [SerializeField] GameObject dotPrefab;
    [SerializeField] int dotCount;
    [SerializeField] float radius;
    [Range(-1, 1)]
    [SerializeField] float minCos;
    RectTransform[] dots;
    public static Crosshair instance { get; private set; }
    new RectTransform transform;
    Camera cam;
    public Vector3 position => mainDot.rectTransform.position;
    public bool AimingNow { get; private set; }
    float angle;
    bool coolDown;
    void Awake()
    {
        cam = Camera.main;
        instance = this;
        transform = GetComponent<RectTransform>();

        dots = new RectTransform[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            var dot = GameObject.Instantiate(dotPrefab, transform);
            var angle = Mathf.PI * 2 * i / dotCount;
            var rect = dot.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            dots[i] = rect;
        }

        mainDot.transform.SetSiblingIndex(transform.childCount - 1);
    }
    private void Start()
    {
        SyncAngleWithPlayerForward();
    }
    public void LateUpdate()
    {
        var targetSpeed = !AimingNow || coolDown ? relaxedSpeed : tensedSpeed;
        var targetScale = !AimingNow || coolDown ? relaxedScale : tensedScale;

        if (outline.rectTransform.localScale.x != targetScale)
        {
            outline.rectTransform.localScale = Vector3.one * Mathf.Lerp(outline.rectTransform.localScale.x, targetScale, Time.deltaTime * targetSpeed / Mathf.Abs(targetScale - outline.rectTransform.localScale.x));
            var color = GetForceState() > 0 ? tensedColor : relaxedColor;
            color.a = GetTenseStateRaw();
            outline.color = color;
            mainDot.color = color;
        }
        else
        {
            coolDown = false;
        }

        ApplyDotSize();
    }
    public float GetTenseStateRaw()
    {
        return Mathf.InverseLerp(relaxedScale, tensedScale, outline.rectTransform.localScale.x);
    }
    public float GetForceState()
    {
        return coolDown ? 0 : Mathf.InverseLerp(minTenseStateToShoot, 1, GetTenseStateRaw());
    }    
    public void SetPosition(Vector3 v)
    {
        transform.position = v;
    }
    public void Translate(Vector2 fingerDelta) //Сколько пикселей пройдено за кадр
    {
        var center = cam.WorldToScreenPoint(Player.ArrowSpawnPosition);
        transform.position = center;
        if (fingerDelta == Vector2.zero)
            return;

        fingerDelta /= Screen.dpi; //сколько дюймов пройдено за кадр

        var d = Vector3.SignedAngle(fingerDelta, mainDot.rectTransform.anchoredPosition, Vector3.back);
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        d *= mouseSensitivityMultiplier;
#endif
        angle += d * sensitivity * fingerDelta.magnitude;

        mainDot.rectTransform.anchoredPosition = new Vector3(Mathf.Sin(angle) * radius, -Mathf.Cos(angle) * radius, 0);
    }
    public void FocusOnPosition(Vector3 worldPosition)
    {
        transform.position = cam.WorldToScreenPoint(Player.ArrowSpawnPosition);
        var toTarget = cam.WorldToScreenPoint(worldPosition) - transform.position;
        toTarget.Normalize();
        mainDot.rectTransform.anchoredPosition = toTarget * radius;
    }
    void ApplyDotSize()
    {
        var mainDotPos = mainDot.rectTransform.anchoredPosition;
        float targetScale;
        foreach (var dot in dots)
        {
            if (AimingNow)
                targetScale = Mathf.InverseLerp(minCos,
                1,
                Vector3.Dot(dot.anchoredPosition, mainDotPos) / radius / radius);
            else
                targetScale = 0;

            var currentScale = dot.localScale.x;
            var d = Mathf.Abs(currentScale - targetScale);
            if (d > 0)
            {
                dot.localScale = Vector3.one * Mathf.Lerp(currentScale, targetScale, Time.deltaTime * dotSpeed / d);
            }
        }
    }
    public void SwitchAim(bool b)
    {
        if (AimingNow == b)
            return;

        AimingNow = b;
        if (AimingNow && (Player.Forward.z * (Mathf.PI - angle)) < 0)
        {
            SyncAngleWithPlayerForward();
        }

        if (!AimingNow)
            coolDown = true;
    }
    void SyncAngleWithPlayerForward()
    {
        angle = Mathf.PI - Mathf.Sign(Player.Forward.z) * Mathf.Deg2Rad * 90;
        mainDot.rectTransform.anchoredPosition = new Vector3(Mathf.Sin(angle) * radius, -Mathf.Cos(angle) * radius, 0);
    }
}
