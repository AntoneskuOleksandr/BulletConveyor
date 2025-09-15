using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject enemyPrefab1;
    [SerializeField] private GameObject enemyPrefab2;

    [Header("Spawn Area")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Inject] private DiContainer _container;
    [Inject] private EnemyManager _enemyManager;

    private float totalSpawnedEnemies = 0;

    /// <summary>
    /// Спавнит указанное количество врагов.
    /// WaveIndex и totalWaves нужны для расчета типа врага.
    /// </summary>
    public int SpawnWave(int enemyCount, int waveIndex, int totalWaves)
    {
        List<Enemy> spawnedEnemies = new List<Enemy>();

        // Рассчитываем процент врагов второго типа
        float progress = Mathf.Clamp01((float)(waveIndex - (totalWaves / 2f)) / (totalWaves / 2f));
        float type2Percent = Mathf.Lerp(0f, 1f, progress);
        float type1Percent = 1f - type2Percent;

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(pointA.position.x, pointB.position.x),
                0f,
                Random.Range(pointA.position.z, pointB.position.z)
            );

            // Определяем тип врага
            GameObject prefabToSpawn = (Random.value < type1Percent) ? enemyPrefab1 : enemyPrefab2;

            GameObject enemyObj = _container.InstantiatePrefab(prefabToSpawn, spawnPosition, Quaternion.identity, null);
            enemyObj.transform.Rotate(0f, 180f, 0f);

            totalSpawnedEnemies++;

            enemyObj.name = $"Enemy({totalSpawnedEnemies})";

            Enemy enemy = enemyObj.GetComponent<Enemy>();
            spawnedEnemies.Add(enemy);
        }

        if (spawnedEnemies.Count > 0)
            _enemyManager.RegisterEnemies(spawnedEnemies);

        return spawnedEnemies.Count;
    }
}
