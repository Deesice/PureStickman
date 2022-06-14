using UnityEngine;

public class Disabler : MonoBehaviour
{
    void Start()
    {
        if (Input.mousePresent)
            gameObject.SetActive(false);
    }
}
