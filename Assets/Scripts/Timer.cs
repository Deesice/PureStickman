using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour, IPool
{
    public bool notCreatedFromPoolManager;
    public float delay;
    public UnityEvent OnTimeLeft;
    public UnityEvent OnReset;

    void Start()
    {
        if (notCreatedFromPoolManager)
            OnTakeFromPool();
    }
    public void OnTakeFromPool()
    {
        OnReset?.Invoke();
        Invoke(nameof(Action), delay);
    }
    private void OnDisable()
    {
        CancelInvoke();
    }
    void Action()
    {
        OnTimeLeft?.Invoke();
    }
    public void ReturnToPool()
    {
        if (notCreatedFromPoolManager)
            return;

        PoolManager.Erase(gameObject);
    }

    public void OnPushToPool()
    {
    }
}
