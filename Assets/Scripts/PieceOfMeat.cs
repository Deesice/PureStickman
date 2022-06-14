using UnityEngine;

public class PieceOfMeat : MonoBehaviour, IPool
{
    [SerializeField] GameObject bloodParticle;
    new Collider collider;
    RendererObserver rendererObserver;
    new Rigidbody rigidbody;
    Vector3 initialScale;
    Quaternion localRotation;
    bool onZombie;
    private void Awake()
    {
        initialScale = transform.localScale;
        collider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();
        rendererObserver = GetComponent<RendererObserver>();
    }
    public void SaveRotation()
    {
        localRotation = transform.localRotation;
    }
    public void OnSpawn()
    {
        onZombie = true;
        collider.enabled = false;
        rigidbody.isKinematic = true;
        rendererObserver.enabled = false;
    }
    public void Launch(Vector3 force)
    {
        onZombie = false;
        transform.parent = null;
        rigidbody.isKinematic = false;
        rendererObserver.enabled = true;
        collider.enabled = true;
        rigidbody.AddForce(force * 0.05f + Vector3.up * 5, ForceMode.VelocityChange);
        rigidbody.angularVelocity = force;
    }
    private void OnCollisionEnter(Collision collision)
    {
        var pos = collision.GetContact(0).point;
        pos.y = 0;
        PoolManager.Create(bloodParticle, pos);
    }

    public void OnTakeFromPool()
    {
        transform.localScale = initialScale;
    }

    public void OnPushToPool()
    {
    }
}
