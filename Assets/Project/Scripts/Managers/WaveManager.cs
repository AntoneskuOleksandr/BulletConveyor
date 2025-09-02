using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int totalWaves = 10;
    [SerializeField] private float waveDuration = 30f;
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Enemy Count Per Wave")]
    [SerializeField] private int minEnemyCount = 3;
    [SerializeField] private int maxEnemyCount = 5;
    [SerializeField] private int enemyIncrementPerWave = 1;

    private int currentWave = 0;
    private int enemiesAlive = 0;

    [Inject] private EnemySpawner _enemySpawner;
    [Inject] private EnemyManager _enemyManager;

    public event Action<float> OnWaveRoutineStarted;   //Время между волнами, количество волн
    public event Action<int, float> OnWaveStarted;     // Номер волны, длительность волны
    public event Action<int> OnWaveCompleted;          // Номер волны
    public event Action<int, int> OnEnemiesChanged;    // alive, total

    public int TotalWaves => totalWaves;

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
            yield return new WaitForSeconds(timeBetweenWaves);

            StartWave(currentWave);

            // Ждём окончания волны: по времени или пока все враги не умерли
            float timer = 0f;
            while (timer < waveDuration && enemiesAlive > 0)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            EndWave(currentWave);
        }
    }

    private void StartWave(int waveIndex)
    {
        // Считаем, сколько врагов нужно спавнить
        int minCount = minEnemyCount + enemyIncrementPerWave * (waveIndex - 1);
        int maxCount = maxEnemyCount + enemyIncrementPerWave * (waveIndex - 1);
        int enemyCount = UnityEngine.Random.Range(minCount, maxCount + 1);

        // Отправляем спавнеру нужное количество врагов и получаем их количество
        enemiesAlive = _enemySpawner.SpawnWave(enemyCount, waveIndex, totalWaves);

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
