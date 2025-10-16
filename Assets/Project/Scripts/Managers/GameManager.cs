using System.Collections;
using System.Linq;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject] private TileManager _tileManager;
    [Inject] private TileGenerator _tileGenerator;
    [Inject] private BulletGenerator _bulletGenerator;
    [Inject] private WaveManager _waveManager;
    [Inject] private WeaponManager _weaponManager;
    [Inject] private UIManager _uiManager;

    [Inject] private LevelLoader _levelLoader;
    [Inject] private PlayerProgressService _progress;

    [SerializeField] private LevelSO[] levels;

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        int currentLevel = _progress.GetSelectedLevel();

        // Находим LevelSO по LevelIndex
        LevelSO levelSO = levels.FirstOrDefault(l => l.LevelIndex == currentLevel);
        if (levelSO == null)
        {
            Debug.LogError($"[GameManager] Level with index {currentLevel} not found!");
            return;
        }

        LevelConfig config = levelSO.levelConfig;
        if (config == null)
        {
            Debug.LogError($"[GameManager] LevelConfig for Level {currentLevel} is null!");
            return;
        }

        Debug.Log($"Starting Level: {config.levelName} (Index: {currentLevel})");
        _levelLoader.LoadLevel(config);

        _tileGenerator.GenerateGrid(6, 6);
        _tileManager.Initialize();
        _weaponManager.InitializeWeapons();
        _bulletGenerator.Initialize();
        _waveManager.Initialize();

        StartCoroutine(_bulletGenerator.StartBulletGeneration());

        _weaponManager.OnAllWeaponsDestroyed += () => StartCoroutine(Lose());
        _waveManager.OnWaveCompleted += wave =>
        {
            if (wave >= _waveManager.TotalWaves)
                StartCoroutine(Win());
        };
    }

    private IEnumerator Win()
    {
        yield return null;
        Time.timeScale = 0f;
        _uiManager.ShowWinPanel();
    }

    private IEnumerator Lose()
    {
        yield return null;
        Time.timeScale = 0f;
        _uiManager.ShowLosePanel();
    }
}
