using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour, IPointerDownHandler
{
    new RectTransform transform;
    [SerializeField] Color selectedColor;
    [SerializeField] Image icon;
    [SerializeField] RectTransform selectorHighlight;
    [SerializeField] float selectorRotationSpeed;
    [SerializeField] Image gradient;
    [SerializeField] Color[] gradientColors;
    [SerializeField] Image[] levelSectors;
    Image background;
    CanvasScaler canvasScaler;
    float lastPointerDownTime;

    Item item;
    private void Awake()
    {
        background = GetComponent<Image>();
        canvasScaler = GetComponentInParent<CanvasScaler>();
        transform = GetComponent<RectTransform>();
        foreach (var i in selectorHighlight.GetComponentsInChildren<Image>())
            i.color = selectedColor;
    }
    private void Update()
    {
        selectorHighlight.rotation *= Quaternion.Euler(0, 0, Time.deltaTime * selectorRotationSpeed);
        if (Input.GetMouseButtonUp(0)
            && Time.time - lastPointerDownTime < 0.3f
            && transform.Contains(Input.mousePosition, canvasScaler))
        {
            ApplyTap();
        }
    }
    public void FillSlot(Item recievedItem)
    {
        item = recievedItem;

        var gradientColor = Color.white;
        if (Inventory.Instance.GetProgress(item) > 0)
        {
            gradientColor = gradientColors[Mathf.Clamp(
                Inventory.Instance.GetProgress(item),
                1,
                gradientColors.Length) - 1];
            icon.sprite = item.UnlockImage;

            for (int i = 0; i < levelSectors.Length; i++)
            {
                levelSectors[i].gameObject.SetActive(item.costPerLevel.Length > 1 && i < Inventory.Instance.GetProgress(item));
            }
        }
        else
        {
            icon.sprite = item.LockImage;
            foreach (var i in levelSectors)
                i.gameObject.SetActive(false);
        }

        gradient.color = gradientColor;
        foreach (var i in levelSectors)
            i.color = gradientColor;

        var meIsSelected = Inventory.Instance.GetEquippedItem(item.SubType) == item;
        selectorHighlight.gameObject.SetActive(meIsSelected);
        background.color = meIsSelected ? selectedColor : Color.white;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        lastPointerDownTime = Time.time;
    }
    void ApplyTap()
    {
        var isDescriptionChanged = Shop.Instance.FillDescription(item);
        Shop.Instance.Select();
        Shop.Instance.FillDescription(item);
        if (isDescriptionChanged)
            UIButton.PlayButtonSound();
    }
}
