using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(SkinnedMeshRenderer))] 
public class HeightFixer : MonoBehaviour, IPool
{
    [SerializeField] bool debug;
    [Range(-0.2f, 0.2f)]
    [SerializeField] float height;
    Animator animator;
    SkinnedMeshRenderer skinnedMeshRenderer;
    bool isVisible;
    [SerializeField] new Transform transform;
    public void OnTakeFromPool()
    {
        isVisible = skinnedMeshRenderer.isVisible;
    }
    private void OnBecameVisible()
    {
        isVisible = true;
    }
    private void OnBecameInvisible()
    {
        isVisible = false;
    }
    private void Awake()
    {
        if (Application.isPlaying)
            debug = false;
        animator = GetComponentInParent<Animator>();
        skinnedMeshRenderer = animator.GetComponentInChildren<SkinnedMeshRenderer>();
    }
    void LateUpdate()
    {
        if (debug || Application.isPlaying && animator.enabled && isVisible)
            transform.position += new Vector3(0, height, 0);
    }
}
