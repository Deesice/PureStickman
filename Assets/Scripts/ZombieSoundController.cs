using UnityEngine;

public class ZombieSoundController : MonoBehaviour, IPool
{
    AudioSource source;
    [SerializeField] Sound ambSound;
    [SerializeField] Sound attackSound;
    [SerializeField] Sound hitSound;
    [SerializeField] Sound deathSound;
    float timeToNextAmbSound;
    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.SetDefaults();
    }
    public void OnTakeFromPool()
    {
        timeToNextAmbSound = ambSound.Play(source).length;
    }
    private void Update()
    {
        timeToNextAmbSound -= Time.deltaTime;
        if (timeToNextAmbSound <= 0)
        {
            OnTakeFromPool();
        }
    }
    public void Attack(float delay)
    {
        SmartInvoke.Invoke(() => attackSound.Play(source), delay);
    }
    public void Hit()
    {
        hitSound.Play(source);
    }
    public void Death()
    {
        source.Stop();
        timeToNextAmbSound = 1000000;
        deathSound.Play(source);
    }
}
