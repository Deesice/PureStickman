using UnityEngine;

public class EasyHeadshotHelper : MonoBehaviour
{
    [SerializeField] SphereCollider headCollider;
    [SerializeField] CapsuleCollider[] armColliders;
    void Start()
    {
        var initialRadius = headCollider.radius;
        foreach (var c in armColliders)
            c.gameObject.layer = 3; //������ ���� Player, ����� ������ ������������ ����

        headCollider.radius = initialRadius + initialRadius / 3 * Inventory.Instance.EffectCount(ItemEffect.EasyHeadshot);
    }
}
