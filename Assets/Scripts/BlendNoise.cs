using UnityEngine;
[RequireComponent(typeof(Animator))]
public class BlendNoise : MonoBehaviour, IPool
{
    Animator animator;
    float blendingSeed;
    [SerializeField] float blendingUpdateSpeed;
    [SerializeField] bool update;

    public void OnPushToPool()
    {
    }

    public void OnTakeFromPool()
    {
        blendingSeed = Random.Range(-1000.0f, 1000.0f);
        animator.SetFloat("Blend", Random.value);
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        OnTakeFromPool();
    }
    void Update()
    {
        if (update)
            animator.SetFloat("Blend", Mathf.PerlinNoise(Time.time * blendingUpdateSpeed, blendingSeed));
    }
}
