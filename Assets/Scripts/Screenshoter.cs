using System;
using UnityEngine;

public class Screenshoter : MonoBehaviour
{
    [SerializeField] KeyCode keyCode;
    [SerializeField] string folderNameInAssets;
    [SerializeField] int superScale;
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(keyCode))
        {
            ScreenCapture.CaptureScreenshot("Assets/" + folderNameInAssets + "/" + DateTime.Now.ToString().GetHashCode().ToString() + ".png", superScale);
        }
#endif
    }
}
