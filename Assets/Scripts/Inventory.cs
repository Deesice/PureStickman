using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : SaveLoadBehaviour<Inventory>
{
    int[] shardsCounts;
    [SerializeField] Item[] items;
    int[] progress;
    Item[] equippedItems;
    public event Action<Item> Equipped;
    public int ItemSubTypeCount { get; private set; }
    public event Action<GoalType, int> ShardsCountChanged;
    public event Action<GoalType> NotEnoughMoneyWarning;
    int[] effectCountTable;
    [ContextMenu("Clear PlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
    public bool IsMoneyTypeUnlocked(GoalType moneyType)
    {
        switch(moneyType)
        {
            case GoalType.Kills:
                return true;
            case GoalType.StayAtPosition:
                return Xp.Instance.Level > 1;
            default:
                return Xp.Instance.Level > 2;
        }
    }
    void SyncWithLevel(int level)
    {
        foreach (var i in items)
            if (GetProgress(i) == 0 && i.minLevelToUnlock > 0 && i.minLevelToUnlock <= level)
                AddLevel(i);

        for (int i = 0; i < Enum.GetValues(typeof(GoalType)).Length; i++)
        {
            if (!IsMoneyTypeUnlocked((GoalType)i))
            {
                AddShards((GoalType)i, shardsCounts[i]);
                shardsCounts[i] = 0;
                ShardsCountChanged?.Invoke((GoalType)i, 0);
            }
        }
    }
    public bool CheckMoney(GoalType moneyType, int count)
    {
        if (GetShardsCount(moneyType) < count)
        {
            NotEnoughMoneyWarning?.Invoke(moneyType);
            return false;
        }
        else
        {
            return true;
        }
    }
    void RefreshEffectCountTable()
    {
        for (int i = 0; i < effectCountTable.Length; i++)
            effectCountTable[i] = 0;

        foreach (var item in equippedItems)
        {
            if (!item)
                continue;
            foreach (var effect in item.effects)
                effectCountTable[(int)effect]++;
        }
    }
    public static void InjectLevelUpEvent(Xp xp)
    {
        xp.LevelUp += Instance.SyncWithLevel;
    }
    private void Start()
    {
        SyncWithLevel(Xp.Instance.Level);
    }
    void EquipDefault()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (progress[i] > 0)
                Equip(items[i]);
        }
    }
    public int GetProgress(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
                return progress[i];
        }
        return 1;
    }
    protected override void Save()
    {
        for (int i = 0; i < progress.Length; i++)
        {
            PlayerPrefs.SetInt("itemlevel" + i, progress[i]);
        }
        for (int i = 0; i < equippedItems.Length; i++)
        {
            for (int j = 0; j < items.Length; j++)
            {
                if (equippedItems[i] == items[j])
                {
                    PlayerPrefs.SetInt("equippedItem" + i, j);
                    break;
                }
            }
        }
        for (int i = 0; i < shardsCounts.Length; i++)
        {
            PlayerPrefs.SetInt("shardscount_" + i, shardsCounts[i]);
        }
    }
    public GoalType ConvertToUnlockedCurrency(GoalType input)
    {
        var idx = (int)input;
        while (!IsMoneyTypeUnlocked((GoalType)idx))
        {
            idx = (idx - 1) % 2;
        }
        return (GoalType)idx;
    }
    public GoalType AddShards(GoalType shardsType, int value)
    {
        var idx = (int)ConvertToUnlockedCurrency(shardsType);
        shardsCounts[idx] += value;
        if (shardsCounts[idx] < 0)
            shardsCounts[idx] = 0;
        ShardsCountChanged?.Invoke(shardsType, shardsCounts[idx]);
        return (GoalType)idx;
    }
    bool ConvertShards(out List<int> outValues)
    {
        int i = 0;
        var output = false;

        outValues = new List<int>();

        for (int j = 0; j < Enum.GetValues(typeof(GoalType)).Length; j++)
        {
            outValues.Add(0);
            outValues.Add(0);
            outValues.Add(0);
            outValues.Add(0);
        }

        while (PlayerPrefs.HasKey("itemprogress" + i))
        {
            var progress = PlayerPrefs.GetFloat("itemprogress" + i, 0);
            var item = items[i];
            if (item && item.costPerLevel != null && item.costPerLevel.Length > 0)
            {
                if (progress > 0)
                {
                    if (progress < 1)
                    {
                        for (int j = 0; j < Enum.GetValues(typeof(GoalType)).Length; j++)
                        {
                            outValues[j] += Mathf.RoundToInt(progress * item.costPerLevel[0][j]);
                        }
                    }
                    else
                    {
                        AddLevel(item);
                    }
                    output = true;
                }
            }
            PlayerPrefs.DeleteKey("itemprogress" + i);
            i++;
        }
        return output;
    }
    void ConvertFrom13(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
            shardsCounts[i] += list[i];

        foreach (var d in FindObjectsOfType<District>(true))
            d.ConvertQuests();
    }
    protected override void Load()
    {
        shardsCounts = new int[Enum.GetValues(typeof(GoalType)).Length];
        ItemSubTypeCount = Enum.GetValues(typeof(Item.ItemSubType)).Length;
        equippedItems = new Item[ItemSubTypeCount];
        effectCountTable = new int[Enum.GetValues(typeof(ItemEffect)).Length];

        progress = new int[items.Length];

        for (int i = 0; i < progress.Length; i++)
        {
            if (items[i].costPerLevel != null && items[i].costPerLevel.Length > 0 || items[i].minLevelToUnlock > 1)
                progress[i] = 0;
            else
                progress[i] = 1;
        }

        EquipDefault();
        ///LOAD///
        if (ConvertShards(out var list))
        {
            SmartInvoke.Invoke(() =>
            ConvertFrom13(list), 0);
        }

        for (int i = 0; i < shardsCounts.Length; i++)
        {
            shardsCounts[i] = PlayerPrefs.GetInt("shardscount_" + i, 0);
#if DEBUG
            shardsCounts[i] += 1000;
#endif
        }

        for (int i = 0; i < progress.Length; i++)
        {
            progress[i] = PlayerPrefs.GetInt("itemlevel" + i, progress[i]);
        }

        for (int i = 0; i < equippedItems.Length; i++)
        {
            if (PlayerPrefs.HasKey("equippedItem" + i))
            {
                var j = PlayerPrefs.GetInt("equippedItem" + i, 0);
                Equip(items[j]);
            }
        }
    }
    public void AddLevel(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                progress[i]++;
                if (item.costPerLevel != null && item.costPerLevel.Length > 0)
                {
                    if (progress[i] > item.costPerLevel.Length)
                        progress[i] = item.costPerLevel.Length;
                }
                else
                {
                    progress[i] = 1;
                }
                return;
            }
        }
    }
    public void Equip(Item item)
    {
        if (equippedItems[(int)item.SubType] == item)
            return;

        equippedItems[(int)item.SubType] = item;
        RefreshEffectCountTable();
        Equipped?.Invoke(item);
    }
    public Item GetEquippedItem(Item.ItemSubType type)
    {
        return equippedItems[(int)type];
    }
    public int EffectCount(ItemEffect wantedEffect)
    {
        var output = effectCountTable[(int)wantedEffect];
        if (output > 1)
            output = 1;
        return output;
    }
    public int GetShardsCount(GoalType shardType)
    {
        return shardsCounts[(int)shardType];
    }
    public void GetItems(List<Item> list, Item.ItemSubType type)
    {
        list.Clear();
        foreach (var i in items)
        {
            if (i.SubType == type)
                list.Add(i);
        }
    }
}
