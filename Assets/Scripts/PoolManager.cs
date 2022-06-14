using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    static PoolManager _instance;
    static PoolManager Instance
    {
        get
        {
            if (!_instance)
                _instance = new GameObject("PoolManager").AddComponent<PoolManager>();
            return _instance;
        }
    }
    Dictionary<GameObject, List<GameObject>> prefabInstances = new Dictionary<GameObject, List<GameObject>>();
    Dictionary<GameObject, IPool[]> prefabComponents = new Dictionary<GameObject, IPool[]>();
    GameObject AddInstance(GameObject prefab)
    {
        var g = GameObject.Instantiate(prefab);
        List<GameObject> list;
        if (prefabInstances.TryGetValue(prefab, out list))
            list.Add(g);
        else
        {
            list = new List<GameObject>();
            list.Add(g);
            prefabInstances.Add(prefab, list);
        }
        g.name += list.Count;
        prefabComponents.Add(g, g.GetComponentsInChildren<IPool>());
        //g.SetActive(false);
        return g;
    }
    public static GameObject Create(GameObject prefab, Vector3 position)
    {
        return Create(prefab, position, Quaternion.identity);
    }

    public static GameObject Create(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (Instance.prefabInstances.TryGetValue(prefab, out var list))
        {
            foreach (var i in list)
                if (!i.activeSelf)
                {
                    i.transform.position = position;
                    i.transform.rotation = rotation;
                    i.SetActive(true);
                    if (Instance.prefabComponents.TryGetValue(i, out var components))
                        foreach (var c in components)
                            c.OnTakeFromPool();
                    return i;
                }
        }
        var g = Instance.AddInstance(prefab);
        //return Create(prefab, position, rotation);
        g.transform.position = position;
        g.transform.rotation = rotation;
        if (Instance.prefabComponents.TryGetValue(g, out var components2))
            foreach (var c in components2)
                c.OnTakeFromPool();
        return g;
    }
    public static void Erase(GameObject destroyableObject)
    {
        if (Instance.prefabComponents.ContainsKey(destroyableObject))
        {
            destroyableObject.SetActive(false);
        }
        else
        {
            GameObject.Destroy(destroyableObject);
        }
    }
}

public interface IPool
{
    public void OnTakeFromPool();
}
