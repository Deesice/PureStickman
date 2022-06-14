using System.Collections.Generic;
using UnityEngine;
public class CameraBehaviour : MonoBehaviour
{
    //[SerializeField] Player target;
    Vector3 initialOffset;
    [SerializeField] Vector3 offset;
    float initialLinearSpeed;
    [SerializeField] float linearSpeed;
    float initialAngularSpeed;
    [SerializeField] float angularSpeed;
    [SerializeField] float tenseLinearOffset;
    [SerializeField] float tenseAngularOffset;
    [SerializeField] float fovStep = 20;
    Vector3 cumulativeScreenShakeOffset;

    Dictionary<Component, float> shakeParams = new Dictionary<Component, float>();
    static CameraBehaviour instance;
    [Header("Creative mode")]
    [SerializeField] bool creativeMode;
    [SerializeField] float maxAplitude = 5;
    [SerializeField] float lerpParameter = 0.5f;
    [SerializeField] float focusSpeed = 0.5f;
    [SerializeField] float fovSpeed = 1;
    [SerializeField] float creativeFovStep = 20;
    float initialFov;
    Vector3 currentToNearestEnemy;
    Camera cam;
    Transform player;
    System.Func<Vector3> TargetPosition;
    public event System.Action Updated;

    Vector3 WantedPosition => TargetPosition() + offset + currentToNearestEnemy;
    Quaternion WantedRotation => Quaternion.LookRotation(TargetPosition() + currentToNearestEnemy - transform.position);
    Vector3 GetPlayerHipsPosition()
    {
        return Player.HipsPosition;
    }
    public void SetTargetPosition(Transform t)
    {
        TargetPosition = () => t.position;
    }
    public void ResetParameters()
    {
        offset = initialOffset;
        linearSpeed = initialLinearSpeed;
        angularSpeed = initialAngularSpeed;
        TargetPosition = GetPlayerHipsPosition;
    }
    public void Zoom(float zoom)
    {
        offset = initialOffset * zoom;
    }
    public void ReflectVerticalOffset()
    {
        offset.y = -offset.y;
    }
    private void Awake()
    {
        initialOffset = offset;
        initialLinearSpeed = linearSpeed;
        initialAngularSpeed = angularSpeed;
        ResetParameters();
        cam = GetComponent<Camera>();
        initialFov = cam.fieldOfView;
        instance = this;

#if !UNITY_EDITOR
    creativeMode = false;
#endif
    }
    private void Start()
    {
        player = FindObjectOfType<Player>().transform;
        transform.position = WantedPosition;
        transform.rotation = WantedRotation;
    }
    void LateUpdate()
    {
        float shakeRadius = 0;
        foreach (var i in shakeParams.Values)
        {
            shakeRadius += i;
        }        

        var tenseOffsetV = Vector3.zero;
        if (creativeMode)
        {
            var nearestEnemy = EnemyRegistrator.GetNearestEnemy(2, player);
            if (nearestEnemy)
            {
                var toNearestEnemy = (nearestEnemy.transform.position - TargetPosition());
                toNearestEnemy.y = 0;
                toNearestEnemy.x = 0;
                toNearestEnemy.z = Mathf.Clamp(toNearestEnemy.z * lerpParameter, -maxAplitude, maxAplitude);
                currentToNearestEnemy = Vector3.Lerp(currentToNearestEnemy, toNearestEnemy, Time.deltaTime * focusSpeed);
                var wantedFov = initialFov + creativeFovStep * Mathf.Abs(toNearestEnemy.z) / maxAplitude;
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wantedFov, Time.deltaTime * fovSpeed);
            }
        }
        else
        {
            cam.fieldOfView = initialFov + Crosshair.instance.GetTenseStateRaw() * fovStep;
            tenseOffsetV.z = -Crosshair.instance.GetTenseStateRaw() * tenseLinearOffset * Mathf.Sign(player.forward.z);
        }

        transform.position = Vector3.Lerp(transform.position - tenseOffsetV - cumulativeScreenShakeOffset, WantedPosition, Time.deltaTime * linearSpeed)
            + tenseOffsetV;
        if (shakeRadius > 0)
        {
            cumulativeScreenShakeOffset = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f),
                UnityEngine.Random.Range(-1.0f, 1.0f),
                UnityEngine.Random.Range(-1.0f, 1.0f)).normalized * shakeRadius;
        }
        else
        {
            cumulativeScreenShakeOffset = Vector3.zero;
        }
        transform.position += cumulativeScreenShakeOffset;
        transform.Rotate(Vector3.up, tenseOffsetV.z * tenseAngularOffset / tenseLinearOffset, Space.World);
        transform.rotation = Quaternion.Slerp(transform.rotation, WantedRotation, Time.deltaTime * angularSpeed);
        transform.Rotate(Vector3.up, -tenseOffsetV.z * tenseAngularOffset / tenseLinearOffset, Space.World);
        Updated?.Invoke();
    }
    public static void SetShakeScreen(float radius, Component sender, float time = 0)
    {
        SmartInvoke.CancelInvoke(sender.GetHierarchyPath() + "cameraShakeParams");
        instance.shakeParams.Remove(sender);
        if (radius > 0)
        {
            instance.shakeParams.Add(sender, radius);
            if (time > 0)
                SmartInvoke.Invoke(() => SetShakeScreen(0, sender), time, sender.GetHierarchyPath() + "cameraShakeParams", true);
        }
    }
}
