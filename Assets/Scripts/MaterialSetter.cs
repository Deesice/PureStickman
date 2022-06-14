using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSetter : MonoBehaviour
{
    public Material material;
    [ContextMenu("Set materials")]
    public void SetMaterials()
    {
        foreach (var r in GetComponentsInChildren<MeshRenderer>())
        {
            r.sharedMaterial = material;
        }
    }
}
