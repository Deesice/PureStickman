using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] float regenSpeed;
    [SerializeField] float maxHealth;
    [Range(0,1)]
    [SerializeField] float ppTresholdNormalized;
    [Header("Pulse properties")]
    [SerializeField] float amplitude;
    [SerializeField] float frequency;
    [SerializeField] float hitAmplitude;
    [SerializeField] float hitTime;
    [Range(0.01f, 1)]
    [SerializeField] float minTimeScale;
    [SerializeField] float shakeRadius;
    float currentHealth;
    Volume healthPP;
    float stunTime;
    public bool Stunned => stunTime > 0;
    public bool IsDead => currentHealth <= 0;
    public event Action<float> Damaged;
    public event Action Dead;
    public event Action<float> Healed;
    void Heal(float value)
    {
        if (value <= 0)
            return;

        stunTime = 0;
        currentHealth += value;
        if (currentHealth > maxHealth)
        {
            value -= currentHealth - maxHealth;
            currentHealth = maxHealth;
        }
        Healed?.Invoke((float)currentHealth / maxHealth);
    }
    private void Awake()
    {
        healthPP = GameObject.FindGameObjectWithTag("HealthPP")?.GetComponent<Volume>();
        if (!healthPP)
            enabled = false;
    }
    private void Start()
    {
        GoalBase.Instance.MissionCompleted += () => stunTime = 1000000;
        EndScreen.Instance.Resurrected += () => Heal(maxHealth - Inventory.Instance.EffectCount(ItemEffect.Armor));
        maxHealth += Inventory.Instance.EffectCount(ItemEffect.Armor);
        Heal(1000000);
    }
    public void AddDamage(float damageValue)
    {
        if (Stunned || damageValue <= 0)
            return;

        currentHealth -= damageValue;
        if (currentHealth > 0)
        {
            stunTime = 0.5f;
            Damaged?.Invoke((float)currentHealth / maxHealth);
        }
        else
        {
            currentHealth = 0;
            stunTime = 1000000;
            Dead?.Invoke();
        }
        StartCoroutine(Hitting());
    }
    IEnumerator Hitting()
    {
        CameraBehaviour.SetShakeScreen(shakeRadius, this, hitTime);
        //Time.timeScale = minTimeScale;
        float i = 0;
        while (i < 1)
        {
            yield return null;
            i += Time.deltaTime / hitTime;
            //Time.timeScale = Mathf.Lerp(minTimeScale, 1, i);
            healthPP.weight += Mathf.Sin(i * Mathf.PI) * hitAmplitude;
        }
    }
    void Update()
    {
        stunTime -= Time.deltaTime;
        if (!IsDead && currentHealth < maxHealth)
        {
            Heal(regenSpeed * Time.deltaTime);
        }

        var wantedWeight = GoalBase.Instance.IsFailed ? 1 : Mathf.InverseLerp(maxHealth * ppTresholdNormalized, 0, currentHealth);
        if (wantedWeight > 0)
                wantedWeight += Mathf.Sin(Time.time * frequency) * amplitude;        
        healthPP.weight = Mathf.Clamp01(wantedWeight);
    }
}
