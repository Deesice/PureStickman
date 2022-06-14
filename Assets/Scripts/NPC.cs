using System.Collections.Generic;
using UnityEngine;
public class NPC : MonoBehaviour, IDamageable
{
    [SerializeField] float alarmDistance;
    [SerializeField] GameObject bloodParticle;
    [SerializeField] int deathAnimationCount;
    [SerializeField] int rotationSpeed;
    public Transform endPosition;
    [SerializeField] float speed;
    Animator animator;
    public event System.Action Dead;
    public event System.Action<int> Damaged;
    bool reached;
    [Header("Sounds")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] Sound deathSound;
    public static List<NPC> AllNPCs = new List<NPC>();
    static NPC rightestNPC;
    [Header("Health")]
    [SerializeField] Vector2Int _maxHealth;
    public int MaxHealth => Mathf.CeilToInt(Mathf.Lerp(_maxHealth.x, _maxHealth.y, DifficultyManager.GetDifficultyGradient()));
    [SerializeField] Sprite _portrait;
    public Sprite Portrait => _portrait;
    public int CurrentHealth { get; private set; }
    static bool zombieIsForward;
    private void Awake()
    {
        AllNPCs.Add(this);
        if (!rightestNPC)
        {
            rightestNPC = this;
        }
        else
        {
            if (rightestNPC.transform.position.z < transform.position.z)
                rightestNPC = this;
        }
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        Compass.AddCompass(transform, CompassType.Defend);
        GetComponent<ZombieTarget>().Eatable = () => CurrentHealth == 0;
        EndScreen.Instance.Resurrected += OnResurrection;
        CurrentHealth = MaxHealth;
        Damaged?.Invoke(MaxHealth);
    }
    void OnResurrection()
    {
        CurrentHealth = MaxHealth;
        Damaged?.Invoke(MaxHealth);
        animator.SetTrigger("resurrection");
    }
    bool IsZombieFrontOfMe(Enemy enemy)
    {
        if (!enemy.IsAlive || !enemy.Ragdoll.isKinematicNow)
            return false;
        
        var toEnemy = enemy.transform.position.z - transform.position.z;
        if (toEnemy > 0 && toEnemy < alarmDistance)
            return true;

        return false;
    }
    void Update()
    {
        if (this == rightestNPC)
        {
            if (reached)
                zombieIsForward = false;
            else
                zombieIsForward = EnemyRegistrator.IsContain(IsZombieFrontOfMe);
        }
        if (reached || GoalBase.Instance.IsComplete || GoalBase.Instance.IsFailed)
        {
            animator.SetFloat("speed", 0);
            return;
        }
        else
        {
            animator.SetFloat("speed", zombieIsForward ? 0 : speed);
        }

        var toEndPosition = endPosition.position - transform.position;
        toEndPosition.y = 0;
        var magnitude = toEndPosition.magnitude;
        if (magnitude > 0)
        {
            transform.position = Vector3.Lerp(transform.position, endPosition.position, Time.deltaTime * (zombieIsForward ? 0 : speed) / magnitude);
            var wantedRotation = Quaternion.LookRotation(toEndPosition);
            var angle = Quaternion.Angle(transform.rotation, wantedRotation);
            if (angle > 0)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toEndPosition), Time.deltaTime * rotationSpeed / angle);
            }
        }
        else
        {
            reached = true;
            GoalBase.Instance.AddValue(GoalType.Escort, 1);
        }
    }
    public void AddDamage(float damageValue)
    {
        if (CurrentHealth == 0)
            return;

        CurrentHealth -= 1;
        Damaged?.Invoke(CurrentHealth);
        deathSound.Play(audioSource);
        PoolManager.Create(bloodParticle, transform.position + Vector3.up);
        if (CurrentHealth == 0)
        {
            animator.ResetTrigger("resurrection");
            animator.SetTrigger("death" + Random.Range(0, deathAnimationCount));
            Dead?.Invoke();
        }
    }
    private void OnDestroy()
    {
        AllNPCs.Remove(this);
    }
}
