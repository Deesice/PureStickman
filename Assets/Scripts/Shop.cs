using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] RectTransform scrollContentTransform;
    [SerializeField] AudioClip buySound;
    [SerializeField] ShopSlot slotPrefab;
    List<ShopSlot> shopSlots = new List<ShopSlot>();
    List<Item> showedItems = new List<Item>();
    public static Shop Instance { get; private set; }
    AudioSource buySoundSource;
    [Header("Description")]
    [SerializeField] Image icon;
    [SerializeField] Text descText;
    [SerializeField] GameObject selector;
    [SerializeField] Text[] moneyValues;
    [SerializeField] GameObject moneyRoot;

    [Header("Buttons")]
    [SerializeField] GameObject buyButton;
    [SerializeField] GameObject enhanceButton;
    [Header("Levels")]
    [SerializeField] Image gradient;
    [SerializeField] Color[] gradientColors;
    [SerializeField] Image[] levelSectors;
    [Header("Tutorial")]
    [SerializeField] Item resetTutorialItem;

    Item item;
    private void Awake()
    {
        Instance = this;
        buySoundSource = gameObject.AddComponent<AudioSource>();
        buySoundSource.playOnAwake = false;
        buySoundSource.clip = buySound;
    }
    private void Start()
    {
        ShowItems(Item.ItemSubType.Character);
    }
    void CreateSlots(int slotCount)
    {
        for (int i = shopSlots.Count; i < slotCount; i++)
        {
            shopSlots.Add(Instantiate(slotPrefab.gameObject, scrollContentTransform).GetComponent<ShopSlot>());
        }
        for (int i = 0; i < slotCount; i++)
        {
            shopSlots[i].gameObject.SetActive(true);
        }
        for (int i = slotCount; i < shopSlots.Count; i++)
        {
            shopSlots[i].gameObject.SetActive(false);
        }

        var wantedHeight = shopSlots[0].GetComponent<RectTransform>().rect.width * slotCount
            + scrollContentTransform.GetComponent<HorizontalLayoutGroup>().spacing * (slotCount - 1);

        scrollContentTransform.sizeDelta = new Vector2(wantedHeight, scrollContentTransform.sizeDelta.y);
        scrollContentTransform.GetComponentInParent<ScrollRect>().horizontalNormalizedPosition = 0;
    }
    void ShowItems(Item.ItemSubType subType)
    {
        Inventory.Instance.GetItems(showedItems, subType);

        showedItems = showedItems.FindAll((i) => !i.hideInShop);

        CreateSlots(showedItems.Count);
        for (int i = 0; i < showedItems.Count; i++)
        {
            shopSlots[i].FillSlot(showedItems[i]);
        }

        FillDescription(Inventory.Instance.GetEquippedItem(subType));
    }
    public void ShowItems(int subType)
    {
        ShowItems((Item.ItemSubType)subType);
    }
    public void PlayBuySound()
    {
        Debug.Log("Play buy sound");
        buySoundSource.Play();
    }
    public bool FillDescription(Item recievedItem)
    {
        if (item == recievedItem)
            return false;

        item = recievedItem;
        if (recievedItem.SubType != Item.ItemSubType.Character)
        {
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

            enhanceButton.SetActive(Inventory.Instance.GetProgress(item) > 0);
            buyButton.SetActive(!(Inventory.Instance.GetProgress(item) > 0));
            descText.text = LangAdapter.FindEntry(item.descCategoryName, item.descEntryName);

            int totalCost = 0;
            if (Inventory.Instance.GetProgress(item) < item.costPerLevel.Length)
            {
                for (int i = 0; i < moneyValues.Length; i++)
                {
                    var currentCost = item.costPerLevel[Inventory.Instance.GetProgress(item)][i];
                    totalCost += currentCost;
                    moneyValues[i].text = currentCost.ToString();
                    moneyValues[i].transform.parent.gameObject.SetActive(currentCost > 0);
                }
            }

            moneyRoot.SetActive(totalCost > 0);
            if (totalCost <= 0)
            {
                enhanceButton.SetActive(false);
                buyButton.SetActive(false);
            }
        }
        else
        {
            moneyRoot.SetActive(false);
            foreach (var i in levelSectors)
                i.gameObject.SetActive(false);
            buyButton.SetActive(false);
            enhanceButton.SetActive(false);
            descText.text = LangAdapter.FindEntry(item.descCategoryName, item.descEntryName);
            if (Inventory.Instance.GetProgress(item) > 0)
            {
                //персонаж открыт
                icon.sprite = item.UnlockImage;
            }
            else
            {
                //персонаж недоступен
                icon.sprite = item.LockImage;
                descText.text += "\n\n" + LangAdapter.FindEntry("Menu", "QuestLocked_start")
                    + item.minLevelToUnlock
                    + LangAdapter.FindEntry("Menu", "QuestLocked_end");
            }
        }
        selector.SetActive(Inventory.Instance.GetEquippedItem(item.SubType) == item);

        if (item == resetTutorialItem)
        {
            Tutorial.Instance.HideState(7);
        }

        return true;
    }
    public void Select()
    {
        if (Inventory.Instance.GetProgress(item) > 0)
            Inventory.Instance.Equip(item);
        var subtype = item.SubType;
        item = null;
        ShowItems(subtype);
    }
    public void TryToBuy()
    {
        bool flag = true;
        for (int i = 0; i < moneyValues.Length; i++)
        {
            flag &= Inventory.Instance.CheckMoney((GoalType)i, item.costPerLevel[Inventory.Instance.GetProgress(item)][i]);
        }
        if (flag)
        {
            for (int i = 0; i < moneyValues.Length; i++)
            {
                Inventory.Instance.AddShards((GoalType)i, -item.costPerLevel[Inventory.Instance.GetProgress(item)][i]);
            }
            Inventory.Instance.AddLevel(item);
            buySoundSource.Play();
            Select();
        }
        else
        {
            UIButton.PlayErrorSound();
        }
    }
}
