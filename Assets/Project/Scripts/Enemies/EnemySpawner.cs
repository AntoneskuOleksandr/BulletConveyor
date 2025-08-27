using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject enemyPrefab1;
    [SerializeField] private GameObject enemyPrefab2;

    [Header("Wave Settings")]
    [SerializeField] private int totalWaves = 10;
    [SerializeField] private float waveDuration = 30f;   // длительность волны
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Enemy Count Per Wave")]
    [SerializeField] private int minEnemyCount = 3;
    [SerializeField] private int maxEnemyCount = 5;
    [SerializeField] private int enemyIncrementPerWave = 1;

    [Header("Spawn Area")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Inject] private DiContainer _container;
    [Inject] private EnemyManager _enemyManager;

    private int currentWave = 0;

    public void Initialize()
    {
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        // Запускаем поочерёдно волны
        for (currentWave = 1; currentWave <= totalWaves; currentWave++)
        {
            SpawnWave(currentWave);

            // Ждём пока идёт волна
            yield return new WaitForSeconds(waveDuration);

            // Перерыв между волнами (если не последняя)
            if (currentWave < totalWaves)
                yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private void SpawnWave(int waveIndex)
    {
        // Увеличиваем количество врагов на каждую волну
        int minCount = minEnemyCount + enemyIncrementPerWave * (waveIndex - 1);
        int maxCount = maxEnemyCount + enemyIncrementPerWave * (waveIndex - 1);

        int enemyCount = Random.Range(minCount, maxCount + 1);

        // Вычисляем соотношение врагов типов 1 и 2
        // В первой половине волн — 100% тип1
        // После середины плавно увеличивается процент тип2
        float progress = Mathf.Clamp01((float)(waveIndex - (totalWaves / 2f)) / (totalWaves / 2f));
        float type2Percent = Mathf.Lerp(0f, 1f, progress);
        float type1Percent = 1f - type2Percent;

        List<Enemy> spawnedEnemies = new List<Enemy>();

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(pointA.position.x, pointB.position.x),
                0f,
                Random.Range(pointA.position.z, pointB.position.z)
            );

            // Определяем, какого типа будет враг
            GameObject prefabToSpawn = (Random.value < type1Percent) ? enemyPrefab1 : enemyPrefab2;

            GameObject enemyObj = _container.InstantiatePrefab(prefabToSpawn, spawnPosition, Quaternion.identity, null);
            enemyObj.transform.Rotate(0f, 180f, 0f); // Поворот врага по оси Y

            Enemy enemy = enemyObj.GetComponent<Enemy>();
            spawnedEnemies.Add(enemy);
        }

        if (spawnedEnemies.Count > 0)
            _enemyManager.RegisterEnemies(spawnedEnemies);
    }
}
