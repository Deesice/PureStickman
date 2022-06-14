using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] Player debugPlayer;
    void Awake()
    {
        if (FindObjectOfType<Player>())
            return;

        if (Inventory.Instance)
        {
            Instantiate(Inventory.Instance.GetEquippedItem(Item.ItemSubType.Character).prefab, MissionManager.SpawnPoint, Quaternion.identity);
        }
        else
        {
            Instantiate(debugPlayer.gameObject, MissionManager.SpawnPoint, Quaternion.identity);
        }
    }
}
