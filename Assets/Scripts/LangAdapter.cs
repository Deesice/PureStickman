using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class LangAdapter : MonoBehaviour
{
    public TextAsset fileToConvert;
    static LangAdapter _instance;
    public static LangAdapter instance
    {
        get { if (_instance == null) { _instance = new GameObject().AddComponent<LangAdapter>();
                SetSystemLanguage(); _instance.gameObject.name = "LangAdapter"; _instance.Load(); } return _instance; }
    }
    public enum Language { English, Russian, Korean, Spanish}
    public static Language CurrentLanguage { get; private set; }
    public event Action OnLanguageChanged;
    static XmlElement root;
    const bool useBuffer = true;
    static Dictionary<string, Dictionary<string, List<string>>> entries;
    static readonly List<string> emptyList = new List<string>();

    string[] pathes = {
        "english",
        "russian",
        "korean",
        "spanish"};
    static Language lastLoadedLanguage;
    void Load()
    {
        if (entries != null)
        {
            if (lastLoadedLanguage == CurrentLanguage)
                return;
        }
        else
        {
            entries = new Dictionary<string, Dictionary<string, List<string>>>();
        }
        lastLoadedLanguage = CurrentLanguage;
        foreach (var d in entries)
            d.Value.Clear();
        {
            XmlDocument mainDoc = new XmlDocument();
            var t = Resources.Load<TextAsset>(pathes[(int)CurrentLanguage]);
            mainDoc.LoadXml(t.text);

            if (useBuffer)
            {
                FillEntries(entries, mainDoc.DocumentElement);
            }
            else
            {
                root = mainDoc.DocumentElement;
            }
        }
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        Debug.Log("Language info loaded successfully");
    }
    public static void SetLanguage(Language language)
    {
        CurrentLanguage = language;
        instance.Load();
        instance.OnLanguageChanged?.Invoke();
    }
    public void NextLanguage()
    {
        SetLanguage((Language)(((int)CurrentLanguage + 1) % 8));
    }
    public void PrevLanguage()
    {
        SetLanguage((Language)(((int)CurrentLanguage + 7) % 8));
    }
    public static string FindEntry(string entryName)
    {
        return FindEntry("main", entryName);
    }
    public static string FindEntry(string categoryName, string entryName)
    {
        instance.Load();
        if (useBuffer)
        {
            if (string.IsNullOrEmpty(categoryName) || string.IsNullOrEmpty(entryName))
                return null;

            if (entries.TryGetValue(categoryName, out var category))
            {
                if (category.TryGetValue(entryName, out var list))
                {
                    return list[0];
                }
            }
        }
        else
        {
            foreach (var i in FindEntries(categoryName, entryName))
                return i;
        }
        return null;
    }
    public static List<string> FindEntries(string categoryName, string entryName)
    {
        instance.Load();
        if (useBuffer)
        {
            if (string.IsNullOrEmpty(categoryName) || string.IsNullOrEmpty(entryName))
                return emptyList;

            if (entries.TryGetValue(categoryName, out var category))
            {
                if (category.TryGetValue(entryName, out var list))
                {
                    return list;
                }
            }
            return emptyList;
        }
        else
        {
            return instance.FindEntries(categoryName, entryName, root);
        }
    }
    List<string> FindEntries(string categoryName, string entryName, XmlElement root)
    {
        List<string> list = new List<string>();
        foreach (XmlNode xnode in root)
        {
            // получаем атрибут name
            if (xnode.Attributes.Count > 0)
            {
                XmlNode attr = xnode.Attributes.GetNamedItem("Name");
                if (attr != null && attr.Value == categoryName)
                {
                    foreach (XmlNode childnode in xnode.ChildNodes)
                    {
                        attr = childnode.Attributes.GetNamedItem("Name");
                        if (attr != null && attr.Value == entryName)
                            foreach (var i in childnode.InnerText.Replace("[voice ", "&[voice ").Replace("[new_page]", "&").Split('&'))
                                if (!string.IsNullOrEmpty(i))
                                    list.Add(i);
                    }
                }
            }
        }
        return list;
    }
    static void FillEntries(Dictionary<string, Dictionary<string, List<string>>> mainDictionary, XmlElement root)
    {
        foreach (XmlNode xnode in root)
        {
            // получаем атрибут name
            if (xnode.Attributes.Count > 0)
            {
                XmlNode attr = xnode.Attributes.GetNamedItem("Name");
                if (attr != null)
                {
                    var categoryName = attr.Value;
                    Dictionary<string, List<string>> categoryDictionary;
                    if (!mainDictionary.TryGetValue(categoryName, out categoryDictionary))
                    {
                        categoryDictionary = new Dictionary<string, List<string>>();
                        mainDictionary.Add(categoryName, categoryDictionary);
                    }
                    foreach (XmlNode childnode in xnode.ChildNodes)
                    {
                        attr = childnode?.Attributes?.GetNamedItem("Name");
                        if (attr != null)
                        {
                            var entryName = attr.Value;
                            if (categoryDictionary.ContainsKey(entryName))
                            {
                                //Debug.LogError("Language category \"" + categoryName + "\" already contain " + entryName + " entry");
                            }
                            else
                            {
                                var list = new List<string>();
                                foreach (var i in childnode.InnerText.Replace("[voice ", "&[voice ").Replace("[new_page]", "&").Split('&'))
                                    if (!string.IsNullOrEmpty(i))
                                        list.Add(ProcessText(i));
                                if (list.Count > 0)
                                    categoryDictionary.Add(entryName, list);
                            }
                        }
                    }
                }
            }
        }
    }
    static string ProcessText(string input, bool processNewStringSymbol = true)
    {
        string output = "";
        foreach (var s in input.Split('[', ']'))
        {
            if (s.Length == 0)
                continue;
            if (s[0] == 'u' && (s[1] == '0' || s[1] >= '1' && s[1] <= '9'))
            {
                output += Char.ConvertFromUtf32(Int32.Parse(s.Replace("u", "")));
                continue;
            }
            if (s == "br" || s.StartsWith("voice") || s == "new_page")
            {
                if (s == "br" && processNewStringSymbol)
                    output += "\n";
                else
                    output += "[" + s + "]";
            }
            else
                output += s;
        }
        return output;
    }
    public static void SetSystemLanguage()
    {
        Language wantedLanguage;
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Belarusian:
            case SystemLanguage.Ukrainian:
            case SystemLanguage.Russian:
                wantedLanguage = LangAdapter.Language.Russian;
                break;
            case SystemLanguage.Korean:
                wantedLanguage = Language.Korean;
                break;
            case SystemLanguage.Spanish:
                wantedLanguage = Language.Spanish;
                break;
            default:
                wantedLanguage = LangAdapter.Language.English;
                break;
        }
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        SetLanguage(LangAdapter.Language.English);
#else
        SetLanguage(wantedLanguage);
#endif
    }
    public void ConvertFile()
    {
#if UNITY_EDITOR
        File.Delete(AssetDatabase.GetAssetPath(fileToConvert).Replace(".txt", "") + "_nounicode.txt");
        File.WriteAllText(AssetDatabase.GetAssetPath(fileToConvert).Replace(".txt", "") + "_nounicode.txt", ProcessText(fileToConvert.text, false));
#endif
    }
}