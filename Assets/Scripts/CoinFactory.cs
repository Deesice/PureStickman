using UnityEngine;

public class CoinFactory : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] float[] probability;
    [SerializeField] float startAcceleration;
    [SerializeField] Coin prefab;
    float currentProbability;
    public float MagnetAcceleration;
    public static CoinFactory Instance { get; private set; }
    AudioSource source;
    [SerializeField] bool debug;
    private void Awake()
    {
#if !UNITY_EDITOR
    debug = false;
#endif
        source = GetComponent<AudioSource>();
        Instance = this;
    }
    private void Start()
    {
        currentProbability = probability.Gradient(DifficultyManager.GetDifficultyGradient());
    }
    public void Collect(Coin coin)
    {
        transform.position = coin.transform.position + Vector3.up;
        source.PlayOneShot(source.clip);
        Inventory.Instance.AddShards(coin.currency, 1);
        //Xp.Instance.SpawnBubble(transform.position, Color.white, BubbleType.PercentFive);
    }
    public void CreateCoin(Vector3 position)
    {
        if (!debug)
        {
            if (currentProbability == 0)
                return;
            if (Random.Range(0.0f, 1.0f) > currentProbability)
                return;
        }

        position.x = 0;
        position.y = 1;
        
        var coin = PoolManager.Create(prefab.gameObject, position).GetComponent<Coin>();
        var angle = Random.Range(60.0f, 120.0f);
        angle *= Mathf.Deg2Rad;
        coin.velocity = new Vector3(0, Mathf.Sin(angle), Mathf.Cos(angle)) * startAcceleration;
    }
}
