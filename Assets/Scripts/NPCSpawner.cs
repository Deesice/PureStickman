using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] NPCPrefabs;
    [SerializeField] Vector2 distanceBetweenInstancesPerDifficulty;
    [SerializeField] Transform endPosition;
    void Awake()
    {
        var currentDistanceBetweenInstances = Mathf.Lerp(distanceBetweenInstancesPerDifficulty.x,
            distanceBetweenInstancesPerDifficulty.y,
            DifficultyManager.GetDifficultyGradient());
        for (int i = 0; i < NPCPrefabs.Length; i++)
        {
            var npc = Instantiate(NPCPrefabs[i],
                MissionManager.SpawnPoint + Vector3.forward * i * currentDistanceBetweenInstances,
                Quaternion.identity).GetComponentInChildren<NPC>();
            if (npc)
            {
                foreach (var healthbar in FindObjectsOfType<NPCHealthbar>(true))
                {
                    healthbar.gameObject.SetActive(true);
                    healthbar.Connect(npc, NPCPrefabs[i]);
                }
                npc.endPosition = endPosition;
            }
        }
    }
}
