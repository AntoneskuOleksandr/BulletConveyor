using UnityEngine;

[CreateAssetMenu(fileName = "New Level Config", menuName = "Scriptable Objects/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Basic Info")]
    public string levelName;

    [Header("Environment")]
    public GameObject environmentPrefab;

    [Header("Waves Settings")]
    [Tooltip("Количество волн на уровне")]
    public int totalWaves = 10;

    [Tooltip("Количество врагов в первой волне")]
    public int baseEnemyCount = 3;

    [Tooltip("Насколько увеличивается количество врагов на каждую следующую волну")]
    public int enemyIncrementPerWave = 1;

    [Tooltip("Дополнительное смещение к максимальному количеству врагов (диапазон)")]
    public int extraEnemyCountRange = 2;

    [Header("Spawn Settings")]
    public GameObject enemyType1;
    public GameObject enemyType2;

    [Header("Audio")]
    public AudioClip backgroundMusic;
}
