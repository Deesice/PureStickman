using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Custom/Item", fileName = "NewItem")]
public class Item : ScriptableObject
{
    public enum ItemSubType { Arrow, Bow, Armor, Character }
    public ItemSubType SubType;
    public string descCategoryName;
    public string descEntryName;
    public Sprite UnlockImage;
    public Sprite LockImage;
    public ItemEffect[] effects;
    public GameObject prefab;
    public Vector4Int[] costPerLevel;
    public bool hideInShop;
    public int minLevelToUnlock;
    public static Sprite GenerateMonochromeImage(Sprite original)
    {
        var origTexture = original.texture;
        var newTexture = new Texture2D(origTexture.width, origTexture.height);
        for (int x = 0; x < newTexture.width; x++)
        {
            for (int y = 0; y < newTexture.height; y++)
            {
                var color = origTexture.GetPixel(x, y);
                var c = color.r * 0.2125f + color.g * 0.7154f + color.b * 0.0721f;
                color.r = c;
                color.g = c;
                color.b = c;
                newTexture.SetPixel(x, y, color);
            }
        }
        newTexture.Apply();
        return Sprite.Create(newTexture, new Rect(0.0f, 0.0f, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
    }
}
[Serializable]
public struct Vector4Int
{
    public int x;
    public int y;
    public int z;
    public int w;
    public int this[int index]
    {
        get
        {
            switch(index)
            {
                case 0:
                    return x;
                case 1:
                    return y;
                case 2:
                    return z;
                case 3:
                    return w;
                default:
                    throw new System.IndexOutOfRangeException();
            }
        }

        set
        {
            switch (index)
            {
                case 0:
                    x = value;
                    return;
                case 1:
                    y = value;
                    return;
                case 2:
                    z = value;
                    return;
                case 3:
                    w = value;
                    return;
                default:
                    throw new System.IndexOutOfRangeException();
            }
        }
    }
}
public enum ItemEffect { DoubleXp, NonStandableEnemies, LegsBreaker, Fire, Perforation, EasyHeadshot, Armor, Explosion }
