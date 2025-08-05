using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Spawner : MonoBehaviour
{
    public static Spawner Instance;
    [Header("Enemy Settings")]
    [SerializeField] private GameObject oldEnemyPrefab;
    [SerializeField] private GameObject newEnemyPrefab;
    [SerializeField] private int minEnemyCount;
    [SerializeField] private int maxEnemyCount;
    [SerializeField] private float spawnCooldown;
    [SerializeField] private float delayBeforeWave = 5f;
    [SerializeField] private float waveHealthIncrease = 10f;
    [SerializeField] private int enemyCountIncrease = 2;

    [Header("Wave Settings")]
    public float waveDuration = 30f;
    public int totalWaves = 5;

    [Header("Spawn Area")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    public bool hasFireInRadiusEffect;
    public bool hasFreezeInRadiusEffect;
    public bool hasPoisonInRadiusEffect;

    public int currentWaveCount = 1;

    public UnityEvent OnNextWave;
    public UnityEvent OnEnemyDeath;

    public List<Enemy> enemies = new List<Enemy>();

    private int enemyID = 0;
    private float spawnTimer = 0f;
    private float waveTimer = 0f;
    private bool isSpawning = true;
    private bool hasWon = false;
    private bool spawn = false;

    private Collider[] colliders;
    private UIManager UIManager;
    private GameObject enemiesFolder;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        spawnTimer = spawnCooldown;

        UIManager = UIManager.Instance;

        enemiesFolder = new GameObject("Enemies");

        if (GameManager.Instance.isTutorial)
            GameManager.Instance.onBlockPlace.AddListener(StartSpawning);
        else
            StartCoroutine(WaveCountdown(delayBeforeWave));
    }

    private void Update()
    {
        if (hasWon)
            return;

        if (!spawn)
            return;

        spawnTimer += Time.deltaTime;
        waveTimer += Time.deltaTime;

        if (isSpawning && spawnTimer >= spawnCooldown)
        {
            Spawn();
            spawnTimer = 0f;
        }

        if (isSpawning && waveTimer >= waveDuration)
        {
            isSpawning = false;
            waveTimer = 0f;
        }

        if (!isSpawning && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            if (currentWaveCount < totalWaves)
            {
                NextWave();
            }
            else
            {
                UIManager.ChangeWaveComingText("Wave completed!");
                GameManager.Instance.Win();
                hasWon = true;
            }
        }
    }

    private IEnumerator WaveCountdown(float countdownTime)
    {
        StartCoroutine(UIManager.WaveCountdownText(countdownTime));
        yield return new WaitForSeconds(countdownTime);
        spawn = true;
    }

    private void NextWave()
    {
        spawn = false;

        currentWaveCount++;
        waveTimer = 0f;
        spawnTimer = spawnCooldown;

        minEnemyCount += enemyCountIncrease;
        maxEnemyCount += enemyCountIncrease;
        isSpawning = true;

        OnNextWave.Invoke();

        UIManager.ChangeWaveSprite();

        if ((currentWaveCount + 1) % 2 == 0)
            UIManager.ShowAbilities();

        StartCoroutine(WaveCountdown(delayBeforeWave));
    }

    private void StartSpawning()
    {
        spawn = true;
    }

    private void Spawn()
    {
        int randomEnemyCount = Random.Range(minEnemyCount, maxEnemyCount);
        int newEnemyCount = 0;
        int oldEnemyCount = randomEnemyCount;

        if (currentWaveCount > 5)
        {
            float newEnemyRatio = (currentWaveCount - 5) * 0.2f;
            newEnemyCount = Mathf.RoundToInt(randomEnemyCount * newEnemyRatio);
            oldEnemyCount = randomEnemyCount - newEnemyCount;
        }

        for (int i = 0; i < oldEnemyCount; i++)
        {
            SpawnEnemy(oldEnemyPrefab);
        }

        for (int i = 0; i < newEnemyCount; i++)
        {
            SpawnEnemy(newEnemyPrefab);
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        enemyID++;
        Vector3 spawnPosition;
        bool isPositionFree;

        int attempts = 0;
        do
        {
            float randomX = Random.Range(pointA.position.x, pointB.position.x);
            float randomZ = Random.Range(pointA.position.z, pointB.position.z);
            spawnPosition = new Vector3(randomX, pointA.position.y, randomZ);

            CapsuleCollider enemyCollider = enemyPrefab.GetComponent<CapsuleCollider>();
            colliders = Physics.OverlapSphere(spawnPosition, enemyCollider.radius);
            isPositionFree = colliders.Length <= 1;

            attempts++;
            if (attempts > 100)
            {
                Debug.Log("Could not find a free position for enemy after 100 attempts");
                return;
            }
        }
        while (!isPositionFree);

        GameObject enemyGO = Instantiate(enemyPrefab, spawnPosition, new Quaternion(0f, 180f, 0, 0f));
        enemyGO.name = "Enemy" + enemyID;
        enemyGO.transform.parent = enemiesFolder.transform;

        Enemy enemy = enemyGO.GetComponent<Enemy>();

        AddEnemyToList(enemy);

        if (enemy != null)
        {
            enemy.maxHealth += currentWaveCount * waveHealthIncrease;

            if (hasFireInRadiusEffect)
                enemy.hasFireInRadiusEffect = true;
            if (hasFreezeInRadiusEffect)
                enemy.hasFreezeInRadiusEffect = true;
            if (hasPoisonInRadiusEffect)
                enemy.hasPoisonInRadiusEffect = true;
        }
    }

    private void AddEnemyToList(Enemy enemy)
    {
        enemies.Add(enemy);
    }

    public void EnemyDied(Enemy enemy)
    {
        enemies.Remove(enemy);
        OnEnemyDeath.Invoke();
    }
}
