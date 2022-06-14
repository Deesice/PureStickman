using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasFollower : MonoBehaviour
{
    [SerializeField] Collider target;
    [SerializeField] Vector2 offset;
    new RectTransform transform;
    static Camera cam;
    CanvasScaler canvasScaler;
    void Awake()
    {
        canvasScaler = GetComponentInParent<CanvasScaler>();
        transform = GetComponent<RectTransform>();
        cam = Camera.main;
    }
    void LateUpdate()
    {
        transform.position = cam.WorldToScreenPoint(target.bounds.center) + (Vector3)offset * canvasScaler.GetRelative4K();
    }
}
