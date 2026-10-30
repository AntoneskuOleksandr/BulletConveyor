using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private int totalWaves;
    [SerializeField] private float waveDuration = 30f;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private int minEnemyCount;
    [SerializeField] private int maxEnemyCount;
    [SerializeField] private int enemyIncrementPerWave;
    private int currentWave;
    private int enemiesAlive;
    private int spawnedThisWave;
    private int waveEnemyCount;
    private bool isFrozen;
    private Coroutine routine;
    [Inject] private EnemySpawner _enemySpawner;
    [Inject] private EnemyManager _enemyManager;
    [Inject] private ProgressBar _progressBar;
    public event Action<float> OnWaveRoutineStarted;
    public event Action<int, float> OnWaveStarted;
    public event Action<int> OnWaveCompleted;
    public event Action<int, float> OnTimerBetweenWavesStarted;
    public event Action<int, int> OnEnemiesChanged;
    public event Action<float, int> OnWaveProgress;
    public int TotalWaves => totalWaves;
    public bool HasStarted => routine != null;

    public void Configure(int total, int baseCount, int increment, int extraRange, GameObject type1, GameObject type2)
    {
        totalWaves = total;
        minEnemyCount = baseCount;
        maxEnemyCount = baseCount + extraRange;
        enemyIncrementPerWave = increment;
        _enemySpawner.Initialize(type1, type2);
        _progressBar.Initialize();
    }

    public void StartWaves()
    {
        if (routine != null) return;
        _enemyManager.OnEnemyDied += HandleEnemyDeath;
        routine = StartCoroutine(WaveRoutine());
    }

    public void Freeze() => isFrozen = true;
    public void Unfreeze() => isFrozen = false;

    private void OnDestroy()
    {
        if (_enemyManager != null) _enemyManager.OnEnemyDied -= HandleEnemyDeath;
    }

    private IEnumerator WaveRoutine()
    {
        OnWaveRoutineStarted?.Invoke(timeBetweenWaves);
        for (currentWave = 1; currentWave <= totalWaves; currentWave++)
        {
            while (isFrozen) yield return null;
            OnTimerBetweenWavesStarted?.Invoke(currentWave, timeBetweenWaves);
            float countdown = timeBetweenWaves;
            while (countdown > 0f)
            {
                if (!isFrozen) countdown -= Time.deltaTime;
                yield return null;
            }
            int increment = enemyIncrementPerWave * (currentWave - 1);
            waveEnemyCount = UnityEngine.Random.Range(minEnemyCount + increment, maxEnemyCount + increment + 1);
            spawnedThisWave = 0;
            OnWaveStarted?.Invoke(currentWave, waveDuration);
            GameAudio.PlayWave();
            yield return _enemySpawner.SpawnWave(waveEnemyCount, currentWave, totalWaves, HandleSpawn);
            while (enemiesAlive > 0) yield return null;
            OnWaveProgress?.Invoke(1f, currentWave);
            OnWaveCompleted?.Invoke(currentWave);
        }
    }

    private void HandleSpawn(Enemy enemy)
    {
        spawnedThisWave++;
        enemiesAlive++;
        OnEnemiesChanged?.Invoke(enemiesAlive, waveEnemyCount);
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        OnEnemiesChanged?.Invoke(enemiesAlive, waveEnemyCount);
        OnWaveProgress?.Invoke((float)(spawnedThisWave - enemiesAlive) / Mathf.Max(1, waveEnemyCount), currentWave);
    }
}
