using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VersionChecker : MonoBehaviour
{
    [SerializeField] UnityEvent OnNewVersionFound;
    string actualVersion;
    void Start()
    {
        actualVersion = ExtensionMethods.GetContentByURL("https://deesice.github.io/aouadactualversion.txt");

        actualVersion = actualVersion.Split(';')[0];
        Debug.Log(actualVersion);

        var currentVersion = Application.version.Split('.');
        int currentVersionIdx = 0;
        int i;
        for (i = 0; i < currentVersion.Length; i++)
        {
            var idx = int.Parse(currentVersion[i]);
            for (int j = i + 1; j < currentVersion.Length; j++)
                idx *= 100;
            currentVersionIdx += idx;
        }
        for (; i < 3; i++)
            currentVersionIdx *= 100;
        
        currentVersion = actualVersion.Split('.');
        int actualVersionIdx = 0;
        for (i = 0; i < currentVersion.Length; i++)
        {
            var idx = int.Parse(currentVersion[i]);
            for (int j = i + 1; j < currentVersion.Length; j++)
                idx *= 100;
            actualVersionIdx += idx;
        }
        for (; i < 3; i++)
            actualVersionIdx *= 100;

        if (currentVersionIdx < actualVersionIdx)
        {
            OnNewVersionFound.Invoke();
        }
    }
    public void SetVersionText(Text text)
    {
        text.text = LangAdapter.FindEntry("Menu", "UpdateAvailable_start")
            + actualVersion
            + LangAdapter.FindEntry("Menu", "UpdateAvailable_end");
    }
}
