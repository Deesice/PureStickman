using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour, IPool
{
    public GoalType currency { get; private set; }
    bool collected;
    [SerializeField] float fadingTime;
    [SerializeField] float endScale;
    [SerializeField] float pseudoColliderRadius;
    SpriteRenderer[] spriteRenderers;
    MeshRenderer[] meshRenderers;
    public Vector3 velocity;

    float[] spriteRenderersInitialScales;
    float[] meshRenderersInitialScales;
    [Header("Technical")]
    [SerializeField] Sprite[] sprites;
    [SerializeField] Color[] colors;

    public void OnTakeFromPool()
    {
        collected = false;

        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].transform.localScale = spriteRenderersInitialScales[i] * Vector3.one;

        for (int i = 0; i < meshRenderers.Length; i++)
            meshRenderers[i].transform.localScale = meshRenderersInitialScales[i] * Vector3.one;

        SetCurrency();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pseudoColliderRadius);
    }
    private void Update()
    {
        if (collected)
            return;

        var toPlayer = (Player.HipsPosition - transform.position).z;
        if (Mathf.Abs(toPlayer) > pseudoColliderRadius)
        {
            var newPosition = transform.position +
                (velocity + Mathf.Sign(toPlayer) * CoinFactory.Instance.MagnetAcceleration * Vector3.forward)
                * Time.deltaTime;
            velocity += Physics.gravity * Time.deltaTime;
            if (newPosition.y < pseudoColliderRadius)
            {
                velocity.y = Mathf.Abs(velocity.y) * 0.5f;
                velocity.x *= 0.01f * Time.deltaTime;
                velocity.z *= 0.01f * Time.deltaTime;
                newPosition.y = pseudoColliderRadius;
            }
            transform.position = newPosition;
        }
        else
        {
            CoinFactory.Instance.Collect(this);
            collected = true;
            StartCoroutine(Fading());
        }
    }
    void SetCurrency()
    {
        var idx = (int)Inventory.Instance.ConvertToUnlockedCurrency((GoalType)Random.Range(0, 4));
        foreach (var r in spriteRenderers)
        {
            r.sprite = sprites[idx];
            r.color = colors[idx];
        }
        currency = (GoalType)idx;
    }
    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();

        spriteRenderersInitialScales = new float[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderersInitialScales[i] = spriteRenderers[i].transform.localScale.x;

        meshRenderersInitialScales = new float[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
            meshRenderersInitialScales[i] = meshRenderers[i].transform.localScale.x;
    }
    IEnumerator Fading()
    {
        var idx = (int)currency;
        //var opacityColor = colors[idx];
        var opacityColor = new Color(1, 1, 1, 0);
        opacityColor.a = 0;
        float i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / fadingTime;
            foreach (var r in spriteRenderers)
                r.color = Color.Lerp(colors[idx], opacityColor, i);

            for (int j = 0; j < meshRenderers.Length; j++)
                meshRenderers[j].transform.localScale = Vector3.one * Mathf.Lerp(meshRenderersInitialScales[j], 0, i);

            for (int j = 0; j < spriteRenderers.Length; j++)
                spriteRenderers[j].transform.localScale = Vector3.one * Mathf.Lerp(spriteRenderersInitialScales[j], spriteRenderersInitialScales[j] * endScale, i);
        }
        PoolManager.Erase(gameObject);
    }

    public void OnPushToPool()
    {
    }
}
