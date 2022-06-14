using UnityEngine;

public class QuestMarker : MonoBehaviour
{
    public Quest quest;
    [Header("Technical")]
    [SerializeField] GameObject lightCylinderPrefab;
    [SerializeField] Vector3 lightCylinderPrefabSpawnOffset;
    [SerializeField] float lightCylinderPrefabScale;
    GameObject lightCylinderInstance;
    [Header("Completed parameters")]
    [SerializeField] MeshRenderer[] renderers;
    [SerializeField] Material completedMaterial;
    private void Start()
    {
        Color lightCylinderColor = Color.white;
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            if (r.sharedMaterial.color != lightCylinderColor)
            {
                lightCylinderColor = r.sharedMaterial.color;
                break;
            }
        }

        if (!quest)
            return;

        if (quest.IsCompleted)
        {
            foreach (var i in renderers)
                i.sharedMaterial = completedMaterial;
            foreach (var i in GetComponentsInChildren<SpriteRenderer>())
            {
                i.color = completedMaterial.color;
                //lightCylinderColor = completedMaterial.color;
            }
        }

        lightCylinderInstance = Instantiate(lightCylinderPrefab, transform.position + lightCylinderPrefabSpawnOffset, Quaternion.identity);
        lightCylinderInstance.GetComponentInChildren<MeshRenderer>().material.color = Color.Lerp(lightCylinderColor, Color.white, 0.5f);
        lightCylinderInstance.AddComponentOrGetIfExists<QuestMarkerLight>().host = this;
        lightCylinderInstance.transform.localScale = lightCylinderPrefabScale * Vector3.one;
    }
    //private void Update()
    //{
    //    lightCylinderInstance.transform.position = initialPosition + lightCylinderPrefabSpawnOffset;
    //    lightCylinderInstance.transform.localScale = lightCylinderPrefabScale * Vector3.one;
    //}
    private void OnEnable()
    {
        if (lightCylinderInstance)
            lightCylinderInstance.gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        if (lightCylinderInstance)
            lightCylinderInstance.gameObject.SetActive(false);
    }
}
