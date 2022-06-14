using System;
using UnityEngine;

public class IKHelper : MonoBehaviour
{
    [HideInInspector] public Vector3 LookPoint;
    public bool focusing { get; private set; }
    [SerializeField] Transform targetBone;
    //[SerializeField] Transform originPosition;
    [SerializeField] Vector3 fixVector;
    [SerializeField] float focusSpeed;
    Quaternion currentRotation;
    public event Action RotationApplied;
    public event Action FocusChanged;
    Quaternion targetRotation;
    private void LateUpdate()
    {
        if (focusing)
        {
            targetRotation = Quaternion.LookRotation(LookPoint - targetBone.position) * Quaternion.Euler(fixVector);
        }
        else
        {
            targetRotation = targetBone.rotation;
        }
        
        currentRotation = Quaternion.Slerp(currentRotation,
            targetRotation,
            Time.deltaTime * focusSpeed);

        targetBone.rotation = currentRotation;
        RotationApplied?.Invoke();
    }
    public void SetFocus(bool newFocus)
    {
        if (focusing == newFocus)
            return;

        focusing = newFocus;
        if (newFocus)
            currentRotation = targetBone.rotation;
        FocusChanged?.Invoke();
    }
}
