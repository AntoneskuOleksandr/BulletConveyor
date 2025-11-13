using System.Collections;
using System.Linq;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LevelSO[] levels;
    [SerializeField] private TutorialManager tutorialManager;

    [Inject] private TileManager _tileManager;
    [Inject] private TileGenerator _tileGenerator;
    [Inject] private ProjectileGenerator _bulletGenerator;
    [Inject] private WaveManager _waveManager;
    [Inject] private WeaponManager _weaponManager;
    [Inject] private UIManager _uiManager;

    [Inject] private LevelLoader _levelLoader;
    [Inject] private WeaponsService _weaponsService;
    [Inject] private PlayerProgressService _playerProgressService;
    [Inject] private TutorialProgressService _tutorialProgress;

    private void Start()
    {
        if (!_tutorialProgress.IsTutorialCompleted)
        {
            tutorialManager.StartTutorial();
            int currentLevel = _playerProgressService.GetSelectedLevel();

            _levelLoader.LoadLevel(levels[0].levelConfig);

            _tileGenerator.GenerateGrid(6, 6);
            _tileManager.Initialize();

            _weaponsService.SelectWeapon(0);

            _weaponManager.InitializeWeapons();
            _bulletGenerator.Initialize();

            StartCoroutine(_bulletGenerator.StartProjectileGeneration());

            _weaponManager.OnAllWeaponsDestroyed += () => StartCoroutine(Lose());
            _waveManager.OnWaveCompleted += wave =>
            {
                if (wave >= _waveManager.TotalWaves)
                    StartCoroutine(Win());
            };
        }
        else
        {
            StartGame();
        }
    }


    public void StartGame()
    {
        int currentLevel = _playerProgressService.GetSelectedLevel();

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

        _levelLoader.LoadLevel(config);

        _tileGenerator.GenerateGrid(6, 6);
        _tileManager.Initialize();
        _weaponManager.InitializeWeapons();
        _bulletGenerator.Initialize();
        _waveManager.StartWaves();

        StartCoroutine(_bulletGenerator.StartProjectileGeneration());

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
