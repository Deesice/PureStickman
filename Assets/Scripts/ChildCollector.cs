using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChildCollector : MonoBehaviour
{
    [SerializeField] string startsWith;
    [ContextMenu("Collect")]
    public void Collect()
    {
        var list = new List<Transform>();
        foreach (var g in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var t in g.GetComponentsInChildren<Transform>())
                if (t.gameObject.name.StartsWith(startsWith))
                    list.Add(t);

        foreach (var t in list)
            t.parent = transform;
    }
}
