using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using Zenject;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Transform environmentRoot;
    private GameObject currentEnvironment;
    [Inject] private WaveManager _waveManager;
    [Inject] private TutorialProgressService _tutorialProgressService;

    public void LoadLevel(LevelConfig config)
    {
        if (currentEnvironment != null)
        {
            foreach (var surface in currentEnvironment.GetComponentsInChildren<NavMeshSurface>()) surface.RemoveData();
            Destroy(currentEnvironment);
        }
        currentEnvironment = Instantiate(config.environmentPrefab, environmentRoot);
        RebuildNavigation();
        GameAudio.PlayMusic(config.backgroundMusic);
        if (_tutorialProgressService.ShouldPlayTutorial)
            _waveManager.Configure(2, 1, 0, 0, config.enemyType1, config.enemyType1, true);
        else
            _waveManager.Configure(config.totalWaves, config.baseEnemyCount, config.enemyIncrementPerWave,
                config.extraEnemyCountRange, config.enemyType1, config.enemyType2);
    }

    public void RebuildNavigation()
    {
        if (currentEnvironment == null) return;
        foreach (var previous in currentEnvironment.GetComponentsInChildren<NavMeshSurface>(true))
        {
            previous.RemoveData();
            previous.enabled = false;
        }
        var surface = currentEnvironment.GetComponent<NavMeshSurface>();
        if (surface == null) surface = currentEnvironment.AddComponent<NavMeshSurface>();
        surface.enabled = true;
        surface.collectObjects = CollectObjects.All;
        surface.layerMask = LayerMask.GetMask("Default", "Environment", "Barrier");
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        Physics.SyncTransforms();
        surface.BuildNavMesh();
    }
}
