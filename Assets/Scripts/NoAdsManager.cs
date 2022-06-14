using UnityEngine;

public class NoAdsManager : MonoBehaviour
{
    [SerializeField] GameObject[] onEnabledAdsOnly;
    static NoAdsManager instance;
    private void Start()
    {
        instance = this;
        foreach (var i in onEnabledAdsOnly)
            i.SetActive(!IsAdsDisabled());
    }
    public static void DisableAds()
    {
        PlayerPrefs.SetInt("noads", 1);
        if (!instance)
            return;

        SmartInvoke.Invoke(() =>
        {
            foreach (var i in instance.onEnabledAdsOnly)
                i.SetActive(!IsAdsDisabled());
        }, 0);
    }
    public static bool IsAdsDisabled()
    {
        return PlayerPrefs.HasKey("noads");
    }
}
