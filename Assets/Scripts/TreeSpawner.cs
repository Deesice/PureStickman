using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
public class TreeSpawner : MonoBehaviour
{
    [SerializeField] float minDistanceBetweenTrees;
    [SerializeField] float radius;
    [SerializeField] GameObject[] prefabs;
    [Range(1, 100)]
    [SerializeField] int reduceTreeFrequency;
    GameObject SpawnTree()
    {
#if UNITY_EDITOR
        var myPos = transform.position;
        myPos.y = 500;
        float angle = Random.Range(0, Mathf.PI * 2);

        var offset = new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
        if (offset.magnitude > radius)
            return null;

        if (Physics.Raycast(myPos + offset, Vector3.down, out var hit))
        {
            if (hit.collider.gameObject.layer != 6)
                return null;

            var g = PrefabUtility.InstantiatePrefab(prefabs.Random()) as GameObject;
            g.transform.position = hit.point;
            g.GetComponent<TreeRandomizer>()?.Randomize();
            return g;
        }
#endif
        return null;
    }
    [ContextMenu("Spawn trees")]
    public void SpawnTrees()
    {
        Clear();
        var list = new List<Transform>();
        int tries = 100;
        while (tries > 0)
        {
            var t = SpawnTree()?.transform;
            if (!t)
                continue;

            var nearEnough = list.Find(p => (p.position - t.position).sqrMagnitude < minDistanceBetweenTrees * minDistanceBetweenTrees);
            if (!nearEnough)
            {
                list.Add(t);
                tries = 100;
            }
            else
            {
                tries--;
                DestroyImmediate(t.gameObject);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    void Clear()
    {
        foreach (var c in Physics.OverlapCapsule(transform.position - Vector3.down * 1000, transform.position + Vector3.down * 1000, radius + minDistanceBetweenTrees * 0.25f))
            if (c.GetComponent<TreeRandomizer>())
                DestroyImmediate(c.gameObject);
    }
    [ContextMenu("Reduce")]
    public void ReduceTrees()
    {
        var allTrees = FindObjectsOfType<TreeRandomizer>().ToList();
        var reduceCount = allTrees.Count / reduceTreeFrequency;
        var candidates = new List<TreeRandomizer>();
        for (int i = 0; i < reduceCount; i++)
        {
            var candidate = allTrees.Random();
            allTrees.Remove(candidate);
            DestroyImmediate(candidate.gameObject);
        }
    }
}
