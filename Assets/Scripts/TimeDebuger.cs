#if UNITY_EDITOR
using UnityEngine;

public class TimeDebuger : MonoBehaviour
{
    [SerializeField] float multiplier;
    int time = 5;
    void Update()
    {
        if (time > 0)
        {
            time -= 1;
        }
        else
        {
            Time.timeScale = multiplier;
        }
    }
}
#endif
