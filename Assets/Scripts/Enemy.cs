using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EnemyPhase { First, Second, Dead}
public enum DamageType { Other, Head, RightLeg, LeftLeg}
public abstract class EnemyState
{
    public bool blockPlayer { get; private set; }
    //protected Transform transform { get; private set; }
    protected Enemy host { get; private set; }
    public EnemyState(Enemy enemy, bool blockPlayer)
    {
        this.blockPlayer = blockPlayer;
        host = enemy;
        //transform = host.transform;
    }
    public abstract void OnUpdate();
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void OnAnimationOver(string expectedTag);
}

[RequireComponent(typeof(Animator))]
public class Enemy : ArrowTarget, IPool
{
    const float firePointCooldown = 0.75f;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    const int maxCorpseCount = 2;
#else
    const int maxCorpseCount = 2;
#endif
    static Queue<Enemy> deadZombies = new Queue<Enemy>();
    public EnemyPhase CurrentPhase { get; private set; }
    [SerializeField] int maxHealth;
    [SerializeField] float _rotationSpeed;
    [SerializeField] float _acceleration;
    [SerializeField] float _deacceleration;
    [SerializeField] Vector2 _healthySpeed;
    float HealthySpeed => Mathf.Max(2, Mathf.Lerp(_healthySpeed.x, _healthySpeed.y, DifficultyManager.GetDifficultyGradient()));
    [SerializeField] float lameSpeed;
    [SerializeField] float _distantToAttack;
    [SerializeField] GameObject bloodParticle;
    [Header("Colliders")]
    [SerializeField] Collider headCollider;
    [SerializeField] Collider[] rightLegColliders;
    [SerializeField] Collider[] leftLegColliders;
    Collider anyOtherColliderForAdditionOtherDamage;
    [Header("Meat")]
    [SerializeField] PieceOfMeat[] meatPrefabs;
    [Range(0, 1)]
    [SerializeField] float meatProbability;
    [SerializeField] float meatMinScale;
    [SerializeField] float meatMaxScale;
    [Header("Features")]
    public bool CanSpawnInPrewarmPhase;
    [SerializeField] bool standableAndRunable;
    [SerializeField] EnemyPhase startPhase;
    [SerializeField] List<DamageType> armor;
    [SerializeField] bool skipFronthandStage;
    public ZombieSoundController ZombieSoundController { get; private set; }
    [Header("Sounds")]
    AudioSource headAudioSource;
    [SerializeField] Sound[] ambSounds;
    [SerializeField] Sound headshotSound;
    Dictionary<Collider, PieceOfMeat> meatMap = new Dictionary<Collider, PieceOfMeat>();
    PlayerWall playerCollider;
    public float Acceleration => _acceleration;
    public float Deacceleration => _deacceleration;
    public float RotationSpeed => _rotationSpeed;
    public float DistantToAttack => _distantToAttack;
    Animator _animator;
    public Animator Animator => _animator;
    EnemyState currentState;
    SelfInitializingList<Rigidbody> _bodies;
    public List<Rigidbody> Rigidbodies => _bodies.GetField(this.GetComponentsInChildren<Rigidbody>);
    //Player player;
    DamageZone damageZone;
    public RagdollHelper Ragdoll { get; private set; }

    public bool isLame;
    int currentHealth;
    public bool IsAlive => currentHealth > 0;
    public Vector3 position => transform.position;
    public Quaternion rotation => transform.rotation;
    bool legsIsBroken;
    bool anyInFire;
    float timeToFirePoints;
    ZombieTarget[] targets;
    public bool IsImmortal { get; private set; }
#if UNITY_EDITOR
    List<string> statesQueue = new List<string>();
#endif
    public bool GetDoubleTimeLegsDamageWhileDeadState { get; private set; }
    [SerializeField] bool passiveBehaviour;
    public Vector3 HeadUppiestPosition => headCollider.bounds.center;
    [SerializeField] Transform compassTarget;
    [SerializeField] float _animationSpeedScale = 1;
    public float AnimationSpeedScale => _animationSpeedScale;
    void Awake()
    {
        EnemyRegistrator.RegisterEnemy(this);
        IsImmortal = true;
        foreach (var damageType in Enum.GetValues(typeof(DamageType)).Cast<DamageType>())
        {
            if (!armor.Contains(damageType))
                IsImmortal &= false;
        }

        ZombieSoundController = GetComponent<ZombieSoundController>();
        headAudioSource = headCollider.gameObject.AddComponent<AudioSource>();
        headAudioSource.SetDefaults();
        playerCollider = GetComponent<PlayerWall>();
        Ragdoll = GetComponent<RagdollHelper>();
        damageZone = GetComponentInChildren<DamageZone>();
        foreach (var c in Colliders)
        {
            anyOtherColliderForAdditionOtherDamage = c;
            if (c != headCollider
                && c != leftLegColliders[0]
                && c != leftLegColliders[1]
                && c != rightLegColliders[0]
                && c != rightLegColliders[1])
            {
                break;
            }
        }
        //player = FindObjectOfType<Player>();
        _animator = GetComponent<Animator>();
        _animator.SetBool("skipFronthandStage", skipFronthandStage);
        //SetCollisionDetectionMode(CollisionDetectionMode.Discrete);
        OnTakeFromPool();
    }
    private void Start()
    {
        targets = FindObjectsOfType<ZombieTarget>();
    }
    void UpdateMeat()
    {
        if (meatProbability == 0)
            return;

        foreach (var i in meatMap.Values)
            PoolManager.Erase(i.gameObject);
        meatMap.Clear();

        foreach (var rb in Rigidbodies)
        {
            if (UnityEngine.Random.Range(0.0f, 1.0f) > meatProbability)
                continue;

            var collider = rb.GetComponent<Collider>();
            var g = PoolManager.Create(meatPrefabs.Random().gameObject, collider.bounds.center);
            var meat = g.GetComponent<PieceOfMeat>();
            meat.OnSpawn();
            g.transform.localScale *= UnityEngine.Random.Range(meatMinScale, meatMaxScale);
            g.transform.parent = rb.transform;
            g.transform.rotation = Quaternion.LookRotation(rb.transform.up);
            g.transform.Rotate(Vector3.up * 90, Space.Self);
            meat.SaveRotation();
            meatMap.Add(collider, meat);
        }
    }
    void SetCollisionDetectionMode(CollisionDetectionMode newMode)
    {
        foreach (var rb in Rigidbodies)
            rb.collisionDetectionMode = newMode;
    }
    public void DisableColliders(bool disable)
    {
        foreach (var rb in Colliders)
            rb.enabled = !disable;
    }
    void Update()
    {
        timeToFirePoints -= Time.deltaTime;
        if (timeToFirePoints < 0)
        {
            timeToFirePoints += firePointCooldown;
            if (anyInFire && (CurrentPhase == EnemyPhase.First || currentHealth > 0))
                Xp.Instance.SpawnXpCrystal(transform.position + Vector3.up);
        }
        currentState.OnUpdate();
    }
    void OnErased()
    {
        foreach (var a in GetComponentsInChildren<Arrow>())
                PoolManager.Erase(a.gameObject);
    }
    public void ChangeState(EnemyState newState)
    {
        if (newState.GetType() == currentState.GetType())
            return;

        currentState.OnExit();
#if UNITY_EDITOR
        statesQueue.Add(currentState.ToString() + " -> " + newState.ToString());
#endif
        currentState = newState;
        playerCollider.enabled = newState.blockPlayer;
        currentState.OnEnter();
    }
    public void Heal()
    {
        currentHealth = maxHealth;
    }
    public void Kill()
    {
        if (IsImmortal) //Телепортируется всегда влево, иметь ввиду!
        {
            transform.position -= new Vector3(0, 0, 10);

            ChangeState(new ToPlayerState(this));       
        }
        else
        {
            currentHealth = 0;
            ZombieSoundController.Death();
            CurrentPhase = EnemyPhase.Dead;
            PoolManager.Create(bloodParticle, headCollider.bounds.center);
            ChangeState(new DeadState(this));
            Death(true, Vector3.zero);
        }
    }
    public void OnTakeFromPool()
    {
        TryToAddCompass();
        GetDoubleTimeLegsDamageWhileDeadState = false;
#if UNITY_EDITOR
        statesQueue.Clear();
#endif
        DisableColliders(false);
        //UpdateMeat();
        anyInFire = false;
        legsIsBroken = false;
        Heal();
        Ragdoll.DisableRagdoll();
        CurrentPhase = startPhase;
        isLame = false;
        currentState = new ToPlayerState(this);
        currentState.OnEnter();
    }
    public void SpecialAnimationOver(string tag)
    {
        currentState.OnAnimationOver(tag);
    }
    void Death(bool isDeathByScipt, Vector3 coinSpawnPos)
    {
        if (!isDeathByScipt)
        {
            CoinFactory.Instance.CreateCoin(coinSpawnPos);
            EndScreen.Instance.AddKill();
            GoalBase.Instance.AddValue(GoalType.Kills, 1);
        }

        deadZombies.Enqueue(this);
        if (deadZombies.Count > maxCorpseCount)
        {
            var zombie = deadZombies.Dequeue();
            PoolManager.Erase(zombie.gameObject);
            zombie.OnErased();
        }

        Compass.RemoveCompass(compassTarget ? compassTarget : transform.Find("mixamorig:Hips"));
    }
    public void AddDamage(DamageType damageType)
    {
        AddDamage(damageType, Vector3.zero);
    }
    public override bool AddDamage(DamageType damageType, Vector3 appliedForce)
    {
        switch (damageType)
        {
            case DamageType.Head:
                return AddDamage(headCollider, appliedForce);
            case DamageType.Other:
                return AddDamage(anyOtherColliderForAdditionOtherDamage, appliedForce);
            default:
                throw new NotImplementedException();
        }
    }
    public override bool AddDamage(Collider hittedCollider, Vector3 appliedForce)
    {
        DamageType damageType = GetDamageType(hittedCollider);
        if (armor.Contains(damageType))
            return false;

        var center = hittedCollider.bounds.center;
        PoolManager.Create(bloodParticle, center);
        anyInFire |= Inventory.Instance.EffectCount(ItemEffect.Fire) > 0;

        //if (meatMap.TryGetValue(hittedCollider, out var meat))
        //{
        //    meat.Launch(appliedForce);
        //    meatMap.Remove(hittedCollider);
        //}

        if (!IsAlive)
            return true;

        if (UnityEngine.Random.Range(0.0f, 1.0f) <= meatProbability)
        {
            var g = PoolManager.Create(meatPrefabs.Random().gameObject, center);
            g.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0, 360),
                UnityEngine.Random.Range(0, 360),
                UnityEngine.Random.Range(0, 360));
            g.transform.localScale = g.transform.localScale * UnityEngine.Random.Range(meatMinScale, meatMaxScale);
            var rb = g.GetComponent<Rigidbody>();
            rb.AddForce(appliedForce * 0.05f + Vector3.up * 5, ForceMode.VelocityChange);
            rb.angularVelocity = appliedForce;
        }

        currentHealth--;

        switch (damageType)
        {
            case DamageType.Head:
                headshotSound.Play(headAudioSource);
                Xp.Instance.SpawnXpCrystal(center, 5);
                break;
            case DamageType.LeftLeg:
            case DamageType.RightLeg:
                Xp.Instance.SpawnXpCrystal(center, 2);
                if (Inventory.Instance.EffectCount(ItemEffect.LegsBreaker) > 0)
                    legsIsBroken = true;
                break;
            default:
                Xp.Instance.SpawnXpCrystal(center);
                break;
        }

        if (damageType == DamageType.Head)
        {
            currentHealth -= 2;
            damageType = DamageType.Other;
        }
        if (!IsAlive)
        {
            ZombieSoundController.Death();
            damageType = DamageType.Head;
        }
        else
        {
            ZombieSoundController.Hit();
        }

        switch (damageType)
        {
            case DamageType.Head:
                currentHealth = 0;
                Animator.SetBool("lame", false);
                isLame = false;
                CurrentPhase = (!standableAndRunable || Inventory.Instance.EffectCount(ItemEffect.NonStandableEnemies) > 0)
                    ? EnemyPhase.Dead : CurrentPhase + 1;
                if (CurrentPhase == EnemyPhase.Dead)
                {
                    Death(false, hittedCollider.bounds.center);
                }
                ChangeState(new DeadState(this));
                break;
            case DamageType.RightLeg:
                Animator.SetFloat("rightLegBroken", 1);
                if (!isLame)
                {
                    if (currentState is AttackState)
                        ChangeState(new FlinchState(this));
                    Animator.SetBool("lame", true);
                    isLame = true;
                }
                else
                {
                    if (!(currentState is DeadState))
                    {
                        ChangeState(new LimpState(this));
                    }
                    else
                    {
                        GetDoubleTimeLegsDamageWhileDeadState = true;
                    }
                }
                break;
            case DamageType.LeftLeg:
                Animator.SetFloat("rightLegBroken", 0);
                if (!isLame)
                {
                    if (currentState is AttackState)
                        ChangeState(new FlinchState(this));
                    Animator.SetBool("lame", true);
                    isLame = true;
                }
                else
                {
                    if (!(currentState is DeadState))
                    {
                        ChangeState(new LimpState(this));
                    }
                    else
                    {
                        GetDoubleTimeLegsDamageWhileDeadState = true;
                    }
                }
                break;
            default:
                if (currentState is DeadState || currentState is WakeUpState || currentState is LimpState)
                    break;
                ChangeState(new FlinchState(this));
                break;
        }
        ApplyImpulse(hittedCollider, appliedForce);
        return true;
    }
    DamageType GetDamageType(Collider hittedCollider)
    {
        if (hittedCollider == headCollider)
            return DamageType.Head;

        foreach (var c in rightLegColliders)
            if (c == hittedCollider)
                return DamageType.RightLeg;

        foreach (var c in leftLegColliders)
            if (c == hittedCollider)
                return DamageType.LeftLeg;

        return DamageType.Other;
    }
    public float CalculateSpeed()
    {
        if (passiveBehaviour)
            return 0;

        if (legsIsBroken || GoalBase.Instance.IsFailed)
            return lameSpeed;

        return CurrentPhase != EnemyPhase.First ? (isLame ? lameSpeed : HealthySpeed) : lameSpeed;
    }
    public void DelegateDamage()
    {
        if (currentState is AttackState)
            damageZone.Attack();
    }
    public void ApplyImpulse(Collider damagedCollider, Vector3 force)
    {
        damagedCollider.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
    }
    public void SetRotation(Quaternion rot)
    {
        //rigidbody.MoveRotation(rot);
        transform.rotation = rot;
    }
    public void SetVelocity(Vector3 velocity)
    {
        transform.position += velocity * Time.deltaTime;
        //rigidbody.velocity = velocity;
    }
    private void OnDestroy()
    {
        EnemyRegistrator.ClearEnemy(this);
        deadZombies.Clear();
    }

    public void OnPushToPool()
    {
    }
    public Vector3 ToNearestTarget()
    {
        return ToNearestTarget(out _);
    }
    public Vector3 ToNearestTarget(out bool eatable)
    {
        eatable = false;
        if (passiveBehaviour)
            return Vector3.zero;
        Vector3 minToTarget = Vector3.zero;
        float minMagnitude = 1000000;
        foreach (var target in targets)
        {
            var v = target.HipsPosition - transform.position;
            v.y = 0;
            var m = v.magnitude;
            if (m < minMagnitude)
            {
                minMagnitude = m;
                minToTarget = v;
                eatable = target.Eatable();
            }
        }
        return minToTarget;
    }
    public void Scream(AudioClip clip)
    {
        headAudioSource.PlayOneShot(clip);
        Scream();
    }
    public void Scream()
    {
        Animator.SetTrigger("scream");
    }
    void TryToAddCompass()
    {
        if (DifficultyManager.GetGoalType() == GoalType.Kills)
            Compass.AddCompass(compassTarget ? compassTarget : transform.Find("mixamorig:Hips"), CompassType.Attack);
    }
}
