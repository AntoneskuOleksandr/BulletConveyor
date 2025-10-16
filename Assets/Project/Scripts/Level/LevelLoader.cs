using UnityEngine;
using Zenject;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Transform environmentRoot;

    private GameObject _currentEnvironment;

    [Inject] private WaveManager _waveManager;

    public void LoadLevel(LevelConfig config)
    {
        Debug.Log("Loading Level...");

        if (config == null)
        {
            Debug.LogError("[LevelLoader] Config is null!");
            return;
        }

        // Удаляем старое окружение
        if (_currentEnvironment != null)
        {
            Debug.Log("Unloading previous environment...");
            Destroy(_currentEnvironment);
        }

        // Подгружаем новое окружение
        if (config.environmentPrefab != null)
        {
            Debug.Log("Loading new environment...");
            _currentEnvironment = Instantiate(config.environmentPrefab, environmentRoot);
        }
        else
            Debug.LogError("Environment prefab is null in LevelConfig.");

        Debug.Log("Configuring wave manager...");
        // Настраиваем волны
        _waveManager.Configure(
            config.totalWaves,
            config.baseEnemyCount,
            config.enemyIncrementPerWave,
            config.extraEnemyCountRange
        );

        Debug.Log($"Level Loaded: {config.levelName}");
    }
}
