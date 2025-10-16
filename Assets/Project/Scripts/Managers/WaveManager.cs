using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int totalWaves;
    [SerializeField] private float waveDuration = 30f;
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Enemy Count Per Wave")]
    [SerializeField] private int minEnemyCount;
    [SerializeField] private int maxEnemyCount;
    [SerializeField] private int enemyIncrementPerWave;

    private int currentWave = 0;
    private int enemiesAlive = 0;

    [Inject] private EnemySpawner _enemySpawner;
    [Inject] private EnemyManager _enemyManager;

    // События
    public event Action<float> OnWaveRoutineStarted;
    public event Action<int, float> OnWaveStarted;
    public event Action<int> OnWaveCompleted;
    public event Action<int, float> OnTimerBetweenWavesStarted;
    public event Action<int, int> OnEnemiesChanged;
    public event Action<float, int> OnWaveProgress;

    public int TotalWaves => totalWaves;

    /// <summary>
    /// Конфигурирует параметры волн на основе LevelConfig.
    /// </summary>
    public void Configure(int total, int baseCount, int increment, int extraRange)
    {
        totalWaves = total;
        minEnemyCount = baseCount;
        maxEnemyCount = baseCount + extraRange;
        enemyIncrementPerWave = increment;
    }

    public void Initialize()
    {
        _enemyManager.OnEnemyDied += HandleEnemyDeath;
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        OnWaveRoutineStarted?.Invoke(timeBetweenWaves);

        for (currentWave = 1; currentWave <= totalWaves; currentWave++)
        {
            OnTimerBetweenWavesStarted?.Invoke(currentWave, timeBetweenWaves);
            yield return new WaitForSeconds(timeBetweenWaves);

            StartWave(currentWave);

            float elapsed = 0f;
            while (elapsed < waveDuration && enemiesAlive > 0)
            {
                elapsed += Time.deltaTime;
                OnWaveProgress?.Invoke(Mathf.Clamp01(elapsed / waveDuration), currentWave);
                yield return null;
            }

            // Если волна закончилась раньше времени
            OnWaveProgress?.Invoke(1f, currentWave);
            EndWave(currentWave);
        }
    }

    private void StartWave(int waveIndex)
    {
        int minCount = minEnemyCount + enemyIncrementPerWave * (waveIndex - 1);
        int maxCount = maxEnemyCount + enemyIncrementPerWave * (waveIndex - 1);
        int enemyCount = UnityEngine.Random.Range(minCount, maxCount + 1);

        enemiesAlive += _enemySpawner.SpawnWave(enemyCount, waveIndex, totalWaves);

        OnWaveStarted?.Invoke(waveIndex, waveDuration);
        OnEnemiesChanged?.Invoke(enemiesAlive, enemiesAlive);
    }

    private void EndWave(int waveIndex)
    {
        OnWaveCompleted?.Invoke(waveIndex);
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        OnEnemiesChanged?.Invoke(enemiesAlive, enemiesAlive);
    }
}
