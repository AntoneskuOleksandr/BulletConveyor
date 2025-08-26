using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab1;
    [SerializeField] private GameObject enemyPrefab2;

    [SerializeField] private int minEnemyCount;
    [SerializeField] private int maxEnemyCount;

    [SerializeField] private float spawnInterval = 3f;

    [Header("Wave Settings")]
    public float waveDuration = 30f;
    public int totalWaves = 5;

    [Header("Spawn Area")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    private List<Enemy> enemies = new List<Enemy>();

    private float spawnTimer;

    [Inject] private DiContainer _container;
    [Inject] private EnemyManager _enemyManager;

    private void Start()
    {
        spawnTimer = spawnInterval;
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        int enemyCount = Random.Range(minEnemyCount, maxEnemyCount + 1);

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(pointA.position.x, pointB.position.x),
                0f,
                Random.Range(pointA.position.z, pointB.position.z)
            );

            GameObject enemyPrefab = (Random.value > 0.5f) ? enemyPrefab1 : enemyPrefab2;
            GameObject enemyObj = _container.InstantiatePrefab(enemyPrefab, spawnPosition, Quaternion.identity, null);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            enemies.Add(enemy);
        }

        if (enemies.Count > 0)
            _enemyManager.RegisterEnemies(enemies);

        enemies.Clear();
    }
}
