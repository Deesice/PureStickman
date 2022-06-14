using UnityEngine;

public class TreeRandomizer : MonoBehaviour
{
    [SerializeField] float minScale;
    [SerializeField] float maxScale;
    [ContextMenu("Randomize")]
    public void Randomize()
    {
        transform.Rotate(Vector3.up * Random.Range(0, 360), Space.World);
        transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
    }
}
