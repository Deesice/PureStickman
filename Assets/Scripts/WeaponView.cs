using UnityEngine;

public class WeaponView : MonoBehaviour
{
    [SerializeField] GameObject armorInstance;
    GameObject bowInstance;
    GameObject arrowInstance;
    IKHelper host;
    Quaternion initialLocalRotation;
    const float translationAcceleraion = 320;
    float translationSpeed;
    private void Awake()
    {
        initialLocalRotation = transform.localRotation;
        host = GetComponentInParent<IKHelper>();
        host.RotationApplied += OnIKRotationApplied;
        host.FocusChanged += () => translationSpeed = 0;
    }
    void Start()
    {
        Inventory.Instance.Equipped += OnItemEquipped;
        for (int i = 0; i < Inventory.Instance.ItemSubTypeCount; i++)
        {
            OnItemEquipped(Inventory.Instance.GetEquippedItem((Item.ItemSubType)i));
        }
        if (FindObjectsOfType<Player>().Length != 1)
            enabled = false;
    }
    [ContextMenu("OnIKRotationApplied")]
    public void OnIKRotationApplied()
    {
        translationSpeed += translationAcceleraion * Time.deltaTime;
        if (host.focusing)
        {
            var wantedRotation = Quaternion.LookRotation(transform.position - Player.RightHandPos, transform.up);
            var angle = Quaternion.Angle(wantedRotation, transform.rotation);
            if (angle > 0)
                transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.deltaTime * translationSpeed / angle);
        }
        else
        {
            var angle = Quaternion.Angle(initialLocalRotation, transform.localRotation);
            if (angle > 0)
                transform.localRotation = Quaternion.Slerp(transform.localRotation, initialLocalRotation, Time.deltaTime * translationSpeed / angle);
        }
    }
    void OnItemEquipped(Item item)
    {
        if (!item.prefab)
            return;

        switch (item.SubType)
        {
            case Item.ItemSubType.Arrow:
                Destroy(arrowInstance);
                arrowInstance = Instantiate(item.prefab, transform);
                foreach (var i in arrowInstance.GetComponentsInChildren<Collider>())
                    DestroyImmediate(i);
                DestroyImmediate(arrowInstance.GetComponentInChildren<Arrow>());
                break;
            case Item.ItemSubType.Bow:
                Destroy(bowInstance);
                bowInstance = Instantiate(item.prefab, transform);
                break;
            case Item.ItemSubType.Armor:
                if (armorInstance)
                    armorInstance.SetActive(item.costPerLevel.Length > 0);
                break;
            default:
                break;
        }
    }
    private void OnDestroy()
    {
        Inventory.Instance.Equipped -= OnItemEquipped;
    }
}
