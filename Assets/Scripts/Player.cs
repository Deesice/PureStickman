using System.Collections.Generic;
using UnityEngine;
public interface ICharacterInput
{
    void PreUpdate();
    bool GetAimingPoint(out Vector3 point);
    float GetMovementInput();
}
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    Animator animator;
    float actualMovementSpeed;
    bool previousAimState;

    [SerializeField] float maxFreeMovementSpeed;
    [SerializeField] float maxAimingMovementSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float deacceleration;
    [SerializeField] float rotationSpeed;
    [SerializeField] float rotationDelay;
    [SerializeField] IKHelper ik;
    [SerializeField] Transform hips;
    [SerializeField] Transform rightHand;
    public static Vector3 RightHandPos => instance.rightHand.position;
    [Header("Arrow")]
    [SerializeField] Transform arrowStartPoint;
    [Header("Sound")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] Sound bowstringSound;
    [SerializeField] Sound deathSound;
    public static Vector3 ArrowSpawnPosition => instance.arrowStartPoint.position;
    Health healthHelper;
    static Player instance;
    public static Vector3 HipsPosition => instance.hips.position;
    public static Vector3 Forward => instance.transform.forward;
    float currentRotationDelay;
    bool RotationDelayed => currentRotationDelay > 0;
    public event System.Action Dead;
    ICharacterInput inputStrategy;
    [SerializeField] BotProperties botProperties;
    void Awake()
    {
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.SetDefaults();
        }

        instance = this;
        animator = GetComponent<Animator>();

        healthHelper = GetComponent<Health>();
        healthHelper.Dead += OnDead;
        healthHelper.Damaged += OnDamaged;

#if UNITY_EDITOR

        if (botProperties)
            inputStrategy = new BotInput(transform, botProperties);
        else
            inputStrategy = new HumanInput();
#else
            inputStrategy = new HumanInput();
#endif

    }
    private void Start()
    {
        GetComponent<ZombieTarget>().Eatable = () => healthHelper.IsDead;
        EndScreen.Instance.Resurrected += OnHealed;
    }
    void OnHealed()
    {
        //if (false)
        //    transform.position = Vector3.zero;
        animator.SetTrigger("resurrection");
    }
    void OnDead()
    {
        deathSound.Play(audioSource);
        Crosshair.instance.SwitchAim(false);
        actualMovementSpeed = 0;
        animator.ResetTrigger("resurrection");
        animator.SetTrigger("death" + Random.Range(1, 8));
        Dead?.Invoke();        
    }
    void OnDamaged(float damageValue)
    {
        deathSound.Play(audioSource);
        actualMovementSpeed = 0;
        animator.SetTrigger("damaged");
    }
    void Aim()
    {
        GetComponentInChildren<BowString>().hooked = true;
        bowstringSound.Play(audioSource);
    }
    void Shoot()
    {
        audioSource.Stop();
        ResetRotationDelay();
        GetComponentInChildren<BowString>().hooked = false;

        var tenseState = Crosshair.instance.GetForceState();
        if (tenseState > 0)
        {
            var spawnPosition = arrowStartPoint.position;
            spawnPosition.x = 0;
            var arrow = PoolManager.Create(Inventory.Instance.GetEquippedItem(Item.ItemSubType.Arrow).prefab,
                spawnPosition,
                Quaternion.LookRotation(ik.LookPoint - spawnPosition, Vector3.up)).GetComponent<Arrow>();
            arrow.Launch(tenseState);
        }
        Crosshair.instance.SwitchAim(false);
    }
    bool IsInvalid()
    {
        return GoalBase.Instance.IsComplete || GoalBase.Instance.IsFailed;
    }
    void ChangeBotMode()
    {
        if (inputStrategy is HumanInput)
            inputStrategy = new BotInput(transform, botProperties);
        else
            inputStrategy = new HumanInput();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            ChangeBotMode();

        inputStrategy.PreUpdate();
        if (IsInvalid())
        {
            GetComponentInChildren<BowString>().hooked = false;
            Crosshair.instance.SwitchAim(false);
            animator.SetFloat("speed", 0);
            animator.SetBool("aim", false);
            ik.SetFocus(false);
            return;
        }

        currentRotationDelay -= Time.deltaTime;

        var isAiming = inputStrategy.GetAimingPoint(out var aimingPoint);
        ik.LookPoint = aimingPoint;

        if (previousAimState != isAiming)
        {
            previousAimState = isAiming;
            if (isAiming)
                Aim();
            else
                Shoot();
        }

        animator.SetBool("aim", isAiming);
        ik.SetFocus(isAiming);

        var wantedSpeed = inputStrategy.GetMovementInput() * (isAiming ? maxAimingMovementSpeed : maxFreeMovementSpeed);
        if (RotationDelayed && (wantedSpeed * transform.forward.z) < 0)
            wantedSpeed = maxAimingMovementSpeed * Mathf.Sign(wantedSpeed);
        if (healthHelper.Stunned)
            wantedSpeed = 0;

        if (actualMovementSpeed != wantedSpeed)
        {
            if (actualMovementSpeed * wantedSpeed < 0)
                ResetRotationDelay();

            var wantedAcceleration = acceleration;
            if (Mathf.Abs(wantedSpeed) < Mathf.Abs(actualMovementSpeed))
                wantedAcceleration = deacceleration;

            actualMovementSpeed = Mathf.Lerp(actualMovementSpeed,
                wantedSpeed,
                Time.deltaTime * wantedAcceleration / Mathf.Abs(actualMovementSpeed - wantedSpeed));
        }

        transform.position += new Vector3(0, 0, actualMovementSpeed * Time.deltaTime);
        var sightDirection = RotationDelayed ? transform.forward.z : actualMovementSpeed;

        if (isAiming)
        {
            sightDirection = (aimingPoint - transform.position).z;
            animator.SetFloat("speed", actualMovementSpeed * Mathf.Sign(sightDirection));
        }
        else
        {
            animator.SetFloat("speed", RotationDelayed ? actualMovementSpeed * Mathf.Sign(sightDirection) : Mathf.Abs(actualMovementSpeed));
        }

        if (Mathf.Abs(sightDirection) > 0)
        {
            var wantedRotation = Quaternion.LookRotation(new Vector3(0, 0, sightDirection));
            var angle = Quaternion.Angle(wantedRotation, transform.rotation);
            if (angle > 0)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.deltaTime * rotationSpeed / angle);
            }
        }
    }
    void ResetRotationDelay()
    {
        currentRotationDelay = rotationDelay;
    }
}
