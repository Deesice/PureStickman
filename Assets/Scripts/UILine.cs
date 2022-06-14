using UnityEngine;
using UnityEngine.UI;

public class UILine : MonoBehaviour
{
    [SerializeField] GameObject linePrefab;
    public Transform target;
    [SerializeField] float width;
    Image line;
    Camera cam;
    new RectTransform transform;
    CanvasScaler canvasScaler;
    private void Awake()
    {
        canvasScaler = GetComponentInParent<CanvasScaler>();
        transform = GetComponent<RectTransform>();
        cam = Camera.main;
        line = Instantiate(linePrefab, transform.parent).GetComponent<Image>();
        line.rectTransform.SetSiblingIndex(transform.GetSiblingIndex());
    }
    private void LateUpdate()
    {
        var dir = cam.WorldToScreenPoint(target.position) - transform.position;
        line.rectTransform.SetPositionAndRotation(Vector3.Lerp(transform.position, transform.position + dir, 0.5f), Quaternion.LookRotation(dir) * Quaternion.Euler(0, 90, 0));
        line.rectTransform.localScale = new Vector3(dir.magnitude / canvasScaler.GetRelative4K(),
            width,
            line.rectTransform.localScale.z);
    }
    private void OnEnable()
    {
        line.gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        line.gameObject.SetActive(false);
    }
}
