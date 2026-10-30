using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using Zenject;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Transform environmentRoot;

    private GameObject _currentEnvironment;

    [Inject] private WaveManager _waveManager;
    [Inject] private TutorialProgressService _tutorialProgressService;

    public void LoadLevel(LevelConfig config)
    {

        if (config == null)
        {
            Debug.LogError("[LevelLoader] Config is null!");
            return;
        }

        if (_currentEnvironment != null)
            Destroy(_currentEnvironment);

        if (config.environmentPrefab != null)
            _currentEnvironment = Instantiate(config.environmentPrefab, environmentRoot);

        else
            Debug.LogError("Environment prefab is null in LevelConfig.");

        if (_currentEnvironment != null && _currentEnvironment.GetComponentInChildren<NavMeshSurface>() == null)
        {
            var surface = _currentEnvironment.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.layerMask = LayerMask.GetMask("Default", "Environment", "Barrier");
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
        }

        GameAudio.PlayMusic(config.backgroundMusic);
        if (_tutorialProgressService.ShouldPlayTutorial)
            _waveManager.Configure(2, 1, 0, 0, config.enemyType1, config.enemyType1);
        else
        {
            _waveManager.Configure(
                config.totalWaves,
                config.baseEnemyCount,
                config.enemyIncrementPerWave,
                config.extraEnemyCountRange,
                config.enemyType1,
                config.enemyType2
            );
        }
    }
}
