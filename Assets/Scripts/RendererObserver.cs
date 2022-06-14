using System;
using UnityEngine;

public class RendererObserver : MonoBehaviour
{
    [SerializeField] Renderer observingRenderer;
    public Action Erased;
    bool flag;
    void Update()
    {
        if (!observingRenderer.isVisible)
        {
            if (!flag)
            {
                flag = true;
            }
            else
            {
                flag = false;
                PoolManager.Erase(gameObject);
                Erased?.Invoke();
            }
        }
        else
        {
            flag = false;
        }
    }
}
