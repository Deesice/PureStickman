using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] Text text;
    float time;
    int fps;
    // Update is called once per frame
    void Update()
    {
        time += Time.unscaledDeltaTime;
        if (time >= 1)
        {
            time -= 1;
            text.text = fps.ToString();
            fps = 0;
        }
        fps++;
    }
}
