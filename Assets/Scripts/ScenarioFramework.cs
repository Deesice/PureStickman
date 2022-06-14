using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScenarioFramework : MonoBehaviour
{
    [SerializeField] UnityEvent[] events;
    [SerializeField] float[] timeStamps;
    int doneEvent;
    float currentTime;
    void Update()
    {
        currentTime += Time.deltaTime / Time.timeScale;
        if (doneEvent < timeStamps.Length && currentTime >= timeStamps[doneEvent])
        {
            events[doneEvent].Invoke();
            doneEvent++;
        }
    }
    public void Equip(Item item)
    {
        Inventory.Instance.Equip(item);
    }
    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }
}
