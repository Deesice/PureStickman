using UnityEngine;

public class XPParticle : MonoBehaviour, IPool
{
    float currentTimeToCollect;
    [SerializeField] float pseudoColliderRadius;
    Vector3 velocity;
    public void OnTakeFromPool()
    {
        var angle = Random.Range(-Mathf.PI, Mathf.PI) / 2;
        var v = new Vector3(0,
            Mathf.Cos(angle),
            Mathf.Sin(angle));
        velocity = v * Xp.Instance.CrystalForce;
        currentTimeToCollect = 0;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pseudoColliderRadius);
    }
    void Move()
    {
        var newPosition = transform.position + velocity * Time.deltaTime;
        velocity += Physics.gravity * Time.deltaTime;
        if (newPosition.y < pseudoColliderRadius)
        {
            velocity.y = Mathf.Abs(velocity.y) * 0.5f;
            newPosition.y = pseudoColliderRadius;
        }
        transform.position = newPosition;
    }
    void Update()
    {
        currentTimeToCollect += Time.deltaTime;
        if (currentTimeToCollect >= Xp.Instance.CrystalTimeToCollect)
        {
            var m = (transform.position - Player.HipsPosition).magnitude;
            if (m > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, Player.HipsPosition, Time.deltaTime * Xp.Instance.CrystalCollectSpeed / m);
            }
            else
            {
                Xp.Instance.AddXp(1);
                PoolManager.Erase(gameObject);
            }
        }
        else
        {
            Move();
        }
    }

    public void OnPushToPool()
    {
    }
}
