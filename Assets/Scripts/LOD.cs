using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LOD : MonoBehaviour
{
    [SerializeField] Mesh mesh;
    [SerializeField] Quaternion rotation;
    void Start()
    {
        GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.rotation = rotation;
    }
}
