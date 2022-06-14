using System.Collections.Generic;
using UnityEngine;
public class ZombieFarm : MonoBehaviour
{
    const float calmZombieSpeed = 0.5f;
    enum WaveMode { SimpleRandom, Waves}
    [SerializeField] ZombieLibrary debugLibrary;
    [SerializeField] ZombieLibrary[] zombieLibraries;
    [SerializeField] Transform[] spawnPoints;
    [Header("(initialSpawnStep; prewarmTime)")]
    [SerializeField] Vector2 easiestSpawnParameters;
    [SerializeField] Vector2 hardestSpawnParameters;
    [SerializeField] WaveMode waveMode;
    [Header("(initialZombiesPerWave; deltaZombiesPerWave; initialTimeBetweenWaves; deltaTimeBetweenWaves)")]
    [SerializeField] Vector4 easiestWaveParameters;
    [SerializeField] Vector4 hardestWaveParameters;
    float positionSpread;
    float timeFromLastSpawn;
    float currentSpawnStep;
    bool prewarmed;
    ZombieLibrary currentLibrary;
    int currentZombiesPerWave;
    Vector4 curWaveParams;
    int currentWave;
    int zombieSpawnedCountPerWave;
    int lastSelectedSpawnPointIdx = 1000000;
    [SerializeField] float usePlayerPosition;
    private void Start()
    {
        SetupDifficulty(DifficultyManager.GetDifficultyGradient());
#if UNITY_EDITOR
        if (debugLibrary)
            currentLibrary = debugLibrary;
#endif
        GetCurrentDifficultyParameters(out var curSpawnParams, out _);
        Prewarm(curSpawnParams.y, calmZombieSpeed);
        EndScreen.Instance.Resurrected += OnResurrected;
        DifficultyManager.Subscribe(SetupDifficulty);
    }
    void SetupDifficulty(float newDifficultyGradient)
    {
        currentLibrary = zombieLibraries.Gradient(newDifficultyGradient);
        positionSpread = newDifficultyGradient;
        GetCurrentDifficultyParameters(out var curSpawnParams, out curWaveParams);
        currentSpawnStep = curSpawnParams.x;
        currentZombiesPerWave = Mathf.RoundToInt(curWaveParams.x);
    }
    void OnResurrected()
    {
        if (false)
        {
            KillAll();

            GetCurrentDifficultyParameters(out var curSpawnParams, out _);
            Prewarm(curSpawnParams.y, calmZombieSpeed);
        }
        else
        {
            foreach (var e in FindObjectsOfType<Enemy>())
            {
                if (e.ToNearestTarget().magnitude <= 10)
                    e.Kill();
            }
        }
    }
    void GetCurrentDifficultyParameters(out Vector2 spawnParams, out Vector4 waveParams)
    {
        spawnParams = Vector2.Lerp(easiestSpawnParameters, hardestSpawnParameters, DifficultyManager.GetDifficultyGradient());
        waveParams = Vector4.Lerp(easiestWaveParameters, hardestWaveParameters, DifficultyManager.GetDifficultyGradient());
        //Debug.Log(spawnParams);
    }
    void Prewarm(float seconds, float speed)
    {
        var list = new List<Vector3>();
        var playerPosition = FindObjectOfType<Player>().transform.position;

        while (true)
        {
            var deltaTime = Mathf.Min(currentSpawnStep - timeFromLastSpawn, seconds);
            seconds -= deltaTime;
            for (int i = 0; i < list.Count; i++)
            {
                list[i] += deltaTime * speed * (playerPosition - list[i]).normalized;
            }
            if (seconds > 0)
            {
                if (SelectSpawnPoint(out var pos))
                    list.Add(pos);
            }
            else
            {
                timeFromLastSpawn += deltaTime;

                foreach (var i in list)
                    PoolManager.Create(currentLibrary.Random(z => z.CanSpawnInPrewarmPhase).gameObject, i);
                return;
            }
        }
    }
    bool SelectSpawnPoint(out Vector3 pos)
    {
        timeFromLastSpawn = 0;
        //currentSpawnStep -= delta;
        if (currentSpawnStep < 1)
            currentSpawnStep = 1;
        switch (waveMode)
        {
            case WaveMode.Waves:
                if (zombieSpawnedCountPerWave >= currentZombiesPerWave)
                {
                    zombieSpawnedCountPerWave++;
                    var gapTime = (zombieSpawnedCountPerWave - currentZombiesPerWave) * calmZombieSpeed;
                    if (gapTime >= curWaveParams.z - currentWave * curWaveParams.w)
                    {
                        currentWave++;
                        zombieSpawnedCountPerWave = 0;
                        currentZombiesPerWave += Mathf.RoundToInt(curWaveParams.y);
                    }
                    pos = Vector3.zero;
                    return false;
                }
                zombieSpawnedCountPerWave++;
                pos = GetRandomSpawnPosition() + new Vector3(0, 0, Random.Range(-positionSpread / 2, positionSpread / 2) * calmZombieSpeed * currentSpawnStep);
                return true;
            default:
                zombieSpawnedCountPerWave++;
                if (usePlayerPosition > 0)
                {
                    pos = GetRandomSpawnPosition();
                }
                else
                {
                    pos = GetRandomSpawnPosition() + new Vector3(0, 0, Random.Range(-positionSpread / 2, positionSpread / 2) * calmZombieSpeed * currentSpawnStep);
                }
                return true;
        }
    }
    Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints.Length > 1)
        {
            var i = lastSelectedSpawnPointIdx;
            while(i == lastSelectedSpawnPointIdx)
            {
                i = Random.Range(0, spawnPoints.Length);
            }
            if (lastSelectedSpawnPointIdx < 0)
                lastSelectedSpawnPointIdx = i;
            else
                lastSelectedSpawnPointIdx = -1;

            if (usePlayerPosition > 0)
            {
                return new Vector3(0, 0, Player.HipsPosition.z + (i * 2 - 1) * usePlayerPosition);
            }
            else
            {
                return spawnPoints[i].position;
            }
        }
        else
        {
            return spawnPoints[0].position;
        }
    }
    void Update()
    {
        //Добавил проверку, чтобы не спавнил после преварма, потому что в первый кадр подскакивает дельтатайм
        if (!prewarmed)
        {
            timeFromLastSpawn += Time.deltaTime;
            if (timeFromLastSpawn >= currentSpawnStep)
            {
                if (SelectSpawnPoint(out var pos))
                    PoolManager.Create(currentLibrary.Random().gameObject, pos);
            }
        }
        else
        {
            prewarmed = false;
        }
    }
    public static void KillAll()
    {
        foreach (var e in FindObjectsOfType<Enemy>())
        {
            e.Kill();
        }
    }
}
