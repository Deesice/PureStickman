using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
[RequireComponent(typeof(Volume))]
public class VolumeController : MonoBehaviour
{
    Volume volume;
    public float TargetValue { get; set; }
    public float Speed { get; set; }
    void Awake()
    {
        volume = GetComponent<Volume>();
    }
    void Update()
    {
        var m = Mathf.Abs(volume.weight - TargetValue);
        if (m > 0)
        {
            volume.weight = Mathf.Lerp(volume.weight, TargetValue, Time.unscaledDeltaTime * Speed / m);
        }
    }
}
