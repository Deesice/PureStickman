using UnityEngine;
[ExecuteInEditMode]
public class Follower : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Quaternion rotationOffset;
    [SerializeField] Vector3 linearOffset;
    private void LateUpdate()
    {
        transform.position = target.position
            + target.forward * linearOffset.z
            + target.right * linearOffset.x
            + target.up * linearOffset.y;
        transform.rotation = target.rotation * rotationOffset;
    }
}
