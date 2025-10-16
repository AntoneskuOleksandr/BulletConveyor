using UnityEngine;

[CreateAssetMenu(fileName = "New Level Config", menuName = "Scriptable Objects/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Basic Info")]
    public string levelName;

    [Header("Environment")]
    public GameObject environmentPrefab;

    [Header("Waves Settings")]
    [Tooltip(" оличество волн на уровне")]
    public int totalWaves = 10;

    [Tooltip(" оличество врагов в первой волне")]
    public int baseEnemyCount = 3;

    [Tooltip("Ќасколько увеличиваетс€ количество врагов на каждую следующую волну")]
    public int enemyIncrementPerWave = 1;

    [Tooltip("ƒополнительное смещение к максимальному количеству врагов (диапазон)")]
    public int extraEnemyCountRange = 2;

    [Header("Audio")]
    public AudioClip backgroundMusic;
}
