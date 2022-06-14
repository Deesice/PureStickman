using System.Collections.Generic;
using UnityEngine;

public class BowString : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] Transform[] points;
    [SerializeField] float radius;
    List<Transform> strings = new List<Transform>();
    public bool hooked;
    void Awake()
    {
        foreach (var p in points)
        {
            var o = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(o.GetComponent<Collider>());
            o.GetComponent<MeshRenderer>().sharedMaterial = material;
            strings.Add(o.transform);
            o.transform.parent = transform;
        }
        GetComponentInParent<IKHelper>().RotationApplied += Draw;
        Draw();
    }
    void Draw()
    {
        Vector3 middlePoint;
        if (hooked)
        {
            middlePoint = Player.RightHandPos - transform.position;
            middlePoint = transform.worldToLocalMatrix * middlePoint;
        }
        else
        {
            middlePoint = (points[0].localPosition + points[1].localPosition) / 2;
        }
        for (int i = 0; i < points.Length; i++)
        {
            var t = strings[i];
            t.localPosition = (middlePoint + points[i].localPosition) / 2;
            var diff = middlePoint - points[i].localPosition;
            t.localRotation = Quaternion.LookRotation(diff);
            t.localScale = new Vector3(radius / transform.lossyScale.x, radius / transform.lossyScale.y, diff.magnitude);
        }
    }
}
