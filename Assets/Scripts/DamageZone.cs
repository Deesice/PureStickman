using UnityEngine;
public interface IDamageable
{
    void AddDamage(float damageValue);
}
public class DamageZone : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] float minLerpParameter;
    [SerializeField] bool ignoreTrigger;
    [SerializeField] float defaultDamage;
    [Header("Capsule parameters")]
    [SerializeField] Transform point1;
    [SerializeField] Transform point2;
    [SerializeField] float capsuleRadius;
    Collider[] targets = new Collider[5];
    static int layerMask;
    private void Awake()
    {
        layerMask = ~LayerMask.GetMask("Corpse");
    }
    public void Attack()
    {
        Attack(defaultDamage);
    }
    public void Attack(float damageValue)
    {
        GenerateCapsule(out var p1, out var p2, out var radius, Mathf.Lerp(minLerpParameter, 1, DifficultyManager.GetDifficultyGradient()));
        var count = Physics.OverlapCapsuleNonAlloc(p1, p2, radius, targets, layerMask, ignoreTrigger ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
        for (int i = 0; i < count; i++)
        {
            Debug.Log(targets[i]);
            targets[i].GetComponentInParent<IDamageable>()?.AddDamage(damageValue);
        }
    }
    private void OnDrawGizmos()
    {
        GenerateCapsule(out var p1, out var p2, out var radius, minLerpParameter);
        Gizmos.DrawWireSphere(p1, radius);
        Gizmos.DrawWireSphere(p2, radius);
    }
    void GenerateCapsule(out Vector3 p1, out Vector3 p2, out float radius, float lerpParameter)
    {
        p1 = Vector3.Lerp(transform.position + Vector3.Project(point1.position, transform.up), point1.position, lerpParameter);
        p2 = Vector3.Lerp(transform.position + Vector3.Project(point2.position, transform.up), point2.position, lerpParameter);
        radius = capsuleRadius * lerpParameter;
    }
    public void Reset()
    {
    }
}
