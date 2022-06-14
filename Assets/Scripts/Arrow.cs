using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour, IPool
{
    new Rigidbody rigidbody;
    [SerializeField] Transform centerOfMass;
    [SerializeField] Vector3 gravity;
    [SerializeField] Vector3 drag;
    [SerializeField] float penetrating;
    [SerializeField] float halfHeight;
    [SerializeField] float radius;
    [SerializeField] GameObject sparkParticle;
    [SerializeField] GameObject[] explosionParticles;
    bool isDangerous;
    new Collider collider;
    static int layerMask;
    int currentPerforatingCount;
    List<ArrowTarget> ignoreEnemies = new List<ArrowTarget>();
    RaycastHit[] rayCastResults;
    Collider[] overlapResults;
    RendererObserver rendererObserver;
    Vector3 launchImpulse;
    Vector3 gravityVelocity;
    [SerializeField] float minLaunchForce;
    [SerializeField] float maxLaunchForce;
    [Header("Sounds")]
    [SerializeField] AudioSource source;
    [SerializeField] Sound launchSound;
    [SerializeField] Sound meatHitSound;
    [SerializeField] Sound armorHitSound;
    [SerializeField] Sound explosionSound;
    ParticleSystem[] initialParticles;
    Camera cam;
    Vector3 CalculateAppliedForce(Collider collider, bool explodeOnly)
    {
        var fromExplode = collider.bounds.center - centerOfMass.position;
        fromExplode.x = 0;
        fromExplode.y = Mathf.Abs(fromExplode.y);
        if (fromExplode.sqrMagnitude > 1)
            fromExplode /= fromExplode.sqrMagnitude;
        else
            fromExplode.Normalize();
        fromExplode *= Inventory.Instance.EffectCount(ItemEffect.Explosion) * 250;
        if (explodeOnly)
            return fromExplode;
        else
            return 5 * (launchImpulse + gravityVelocity) + fromExplode;
    }
    void Awake()
    {
        rayCastResults = new RaycastHit[Inventory.Instance.EffectCount(ItemEffect.Perforation) * 11 + 1];
        cam = Camera.main;
        initialParticles = GetComponentsInChildren<ParticleSystem>();
        if (Inventory.Instance.EffectCount(ItemEffect.Explosion) > 0)
        {
            overlapResults = new Collider[11 * 10];
        }
        rendererObserver = GetComponent<RendererObserver>();
        layerMask = LayerMask.GetMask("Corpse");
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.centerOfMass = centerOfMass.localPosition;
        foreach (var c in GetComponentsInChildren<Collider>())
            if (c.enabled && !c.isTrigger)
                collider = c;
    }
    private void FixedUpdate()
    {
        if (isDangerous)
        {
            gravityVelocity += Physics.gravity * Time.fixedDeltaTime;
            var wantedVelocity = launchImpulse + gravityVelocity;
            rigidbody.MovePosition(rigidbody.position + wantedVelocity * Time.fixedDeltaTime);
            rigidbody.MoveRotation(Quaternion.LookRotation(wantedVelocity, Vector3.up));
        }
    }
    public void Launch(float forceValue)
    {
        gravityVelocity = Vector3.zero;
        launchImpulse = transform.forward * Mathf.Lerp(minLaunchForce, maxLaunchForce, forceValue);
        launchSound.Play(source, forceValue);
        rigidbody.useGravity = false;
        isDangerous = true;
    }
    //private void Update()
    //{
    //    if (!isDangerous)
    //        return;

    //    var count = Physics.OverlapCapsuleNonAlloc(centerOfMass.position + centerOfMass.up * halfHeight,
    //        centerOfMass.position - centerOfMass.up * halfHeight,
    //        radius,
    //        scanResults, 7, QueryTriggerInteraction.Ignore); //сканируем только слой Corpse

    //    Debug.Log(count);

    //    for (int i = 0; i < count; i++)
    //    {
    //        Debug.Log("Try process: " + scanResults[i].gameObject);
    //        OnTriggerEnter(scanResults[i]);
    //    }
    //}
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(centerOfMass.position + centerOfMass.up * halfHeight, radius);
        Gizmos.DrawSphere(centerOfMass.position - centerOfMass.up * halfHeight, radius);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!isDangerous)
            return;

        var enemy = collision.gameObject.GetComponentInParent<ArrowTarget>();
        if (!enemy)
        {
            rigidbody.useGravity = true;
            rigidbody.velocity = Vector3.zero;
            isDangerous = false;
            rendererObserver.enabled = true;
            var forceDirection = (launchImpulse + gravityVelocity) * 0.25f;
            forceDirection.y = Mathf.Abs(forceDirection.y);
            if (Inventory.Instance.EffectCount(ItemEffect.Explosion) > 0)
            {
                forceDirection *= Inventory.Instance.EffectCount(ItemEffect.Explosion) + 1;
                Explode(true);
            }
            rigidbody.AddForceAtPosition(forceDirection,
                centerOfMass.position, ForceMode.VelocityChange);
        }
        return;
    }
    void Explode(bool playMeatSound)
    {
        CameraBehaviour.SetShakeScreen(
            0.125f
            * Inventory.Instance.EffectCount(ItemEffect.Explosion)
            * Mathf.InverseLerp(
                source.maxDistance,
                source.minDistance,
                (cam.transform.position - centerOfMass.position).magnitude),
            this, 0.4f);
        explosionSound.Play(source, Mathf.InverseLerp(minLaunchForce, maxLaunchForce, launchImpulse.magnitude));
        foreach (var p in initialParticles)
            p.gameObject.SetActive(false);
        var radius = Inventory.Instance.EffectCount(ItemEffect.Explosion) * 1.5f;
        PoolManager.Create(explosionParticles.Random(), centerOfMass.position).transform.localScale = Vector3.one * radius * 2;
        var count = Physics.OverlapSphereNonAlloc(centerOfMass.position, radius, overlapResults, layerMask);
        for (int i = 0; i < count; i++)
        {
            var collider = overlapResults[i];
            var flag = false;
            foreach (var e in ignoreEnemies)
                if (e.Colliders.Contains(collider))
                {
                    flag = true;
                    break;
                }
            if (flag)
                continue;

            var enemy = collider.GetComponentInParent<ArrowTarget>();
            if (!enemy)
            {
                continue;
            }
            ignoreEnemies.Add(enemy);
            enemy.AddDamage(DamageType.Other, CalculateAppliedForce(collider, true));
            if (playMeatSound)
            {
                playMeatSound = false;
                meatHitSound.Play(source, Mathf.InverseLerp(minLaunchForce, maxLaunchForce, launchImpulse.magnitude));
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        foreach (var e in ignoreEnemies)
            if (e.Colliders.Contains(other))
                return;

        if (!isDangerous || other.isTrigger)
            return;

        var enemy = other.gameObject.GetComponentInParent<ArrowTarget>();
        if (!enemy)
        {
            return;
        }

        var fixPosition = other.bounds.center;

        //Рейкастим из хвоста стрелы
        var count = Physics.RaycastNonAlloc(transform.position - centerOfMass.position + transform.position,
            transform.forward,
            rayCastResults,
            1,
            layerMask,
            QueryTriggerInteraction.Ignore);

        var sparkNormal = -transform.forward;
        for (int i = 0; i < count; i++)
        {
            var hit = rayCastResults[i];
            var flag = false;
            foreach (var e in ignoreEnemies)
                if (e.Colliders.Contains(hit.collider))
                {
                    flag = true;
                    break;
                }
            if (flag)
                continue;

            if (hit.collider.gameObject.GetComponentInParent<ArrowTarget>())
            {
                other = hit.collider;
                sparkNormal = hit.normal;
                fixPosition = hit.point;
                break;
            }
        }

        bool enemyDamaged;
        if (Inventory.Instance.EffectCount(ItemEffect.Explosion) > 0)
            enemyDamaged = enemy.AddDamage(DamageType.Head, CalculateAppliedForce(other, false));
        else
            enemyDamaged = enemy.AddDamage(other, CalculateAppliedForce(other, false));

        if (!enemyDamaged)
        {
            PoolManager.Create(sparkParticle, fixPosition, Quaternion.LookRotation(sparkNormal)
                );
            rigidbody.useGravity = true;
            isDangerous = false;
            rendererObserver.enabled = true;
            rigidbody.velocity = Vector3.zero;
            rigidbody.AddForceAtPosition(-transform.forward * 2, centerOfMass.position, ForceMode.VelocityChange);
            armorHitSound.Play(source, Mathf.InverseLerp(minLaunchForce, maxLaunchForce, launchImpulse.magnitude));
            if (Inventory.Instance.EffectCount(ItemEffect.Explosion) > 0)
                Explode(true);
            return;
        }

        meatHitSound.Play(source, Mathf.InverseLerp(minLaunchForce, maxLaunchForce, launchImpulse.magnitude));
        ignoreEnemies.Add(enemy);

        if (currentPerforatingCount > 0)
        {
            currentPerforatingCount--;
            return;
        }

        if (Inventory.Instance.EffectCount(ItemEffect.Explosion) > 0)
            Explode(false);
        else
        {
            CameraBehaviour.SetShakeScreen(
                0.05f * Mathf.InverseLerp(
                    source.maxDistance,
                    source.minDistance,
                    (cam.transform.position - centerOfMass.position).magnitude),
                this, 0.2f);
        }

        if (enemy.StickToCenter)
            fixPosition = other.bounds.center;

        transform.position = fixPosition
            - transform.LocalToWorld(centerOfMass.localPosition)
            + transform.forward * penetrating;

        isDangerous = false;
        if (collider)
            collider.enabled = false;
        transform.parent = other.transform;
        rigidbody.isKinematic = true;
    }
    public void OnTakeFromPool()
    {
        foreach (var p in initialParticles)
            p.gameObject.SetActive(true);
        rigidbody.velocity = Vector3.zero;
        rendererObserver.enabled = false;
        ignoreEnemies.Clear();
        currentPerforatingCount = Inventory.Instance.EffectCount(ItemEffect.Perforation);
        rigidbody.isKinematic = false;
        isDangerous = false;
        if (collider)
            collider.enabled = true;
        transform.parent = null;
    }

    public void OnPushToPool()
    {
        collider.enabled = false;
        rigidbody.isKinematic = true;
    }
}
