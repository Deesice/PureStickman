using UnityEngine;
using UnityEngine.UI;

public class PreloadText : MonoBehaviour
{
    Text text;
    public string category;
    public string entry;
    public string template;
    void Start()
    {
        text = GetComponent<Text>();
        SetupText();
        LangAdapter.instance.OnLanguageChanged += SetupText;
    }
    void SetupText()
    {
        var translatedEntry = LangAdapter.FindEntry(category, entry);
        if (string.IsNullOrEmpty(template))
            text.text = translatedEntry;
        else
            text.text = template.Replace("*", translatedEntry);
    }
}
