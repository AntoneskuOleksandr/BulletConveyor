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
    [Inject] private GameSpeed _gameSpeed;
    private bool resultShown;
    public bool IsTrainingReplay { get; private set; }

    private void Start() => StartGame();

    public void StartGame()
    {
        bool training = _tutorialProgress.ShouldPlayTutorial;
        IsTrainingReplay = training && _tutorialProgress.IsTutorialCompleted;
        int selected = training ? levels[0].LevelIndex : _playerProgressService.GetSelectedLevel();
        var level = levels.FirstOrDefault(l => l.LevelIndex == selected && l.levelConfig != null);
        if (level == null) level = levels.First(l => l.levelConfig != null);
        if (training || _weaponsService.GetSelectedCount() == 0) _weaponsService.SelectWeapon(0);
        _levelLoader.LoadLevel(level.levelConfig);
        _tileGenerator.GenerateGrid(6, 6);
        _tileManager.Initialize();
        _weaponManager.InitializeWeapons();
        _bulletGenerator.Initialize();
        _weaponManager.OnAllWeaponsDestroyed += HandleDefeat;
        _waveManager.OnWaveCompleted += HandleWaveCompleted;
        StartCoroutine(_bulletGenerator.StartProjectileGeneration());
        if (training) tutorialManager.StartTutorial();
        else
        {
            tutorialManager.HideTutorial();
            _waveManager.StartWaves();
        }
    }

    private void OnDestroy()
    {
        _weaponManager.OnAllWeaponsDestroyed -= HandleDefeat;
        _waveManager.OnWaveCompleted -= HandleWaveCompleted;
    }

    private void HandleWaveCompleted(int wave)
    {
        if (wave >= _waveManager.TotalWaves) StartCoroutine(Finish(true));
    }

    private void HandleDefeat() => StartCoroutine(Finish(false));

    private IEnumerator Finish(bool victory)
    {
        if (resultShown) yield break;
        resultShown = true;
        yield return null;
        _gameSpeed.Pause(this);
        GameAudio.PlayResult(victory);
        if (victory) _uiManager.ShowWinPanel();
        else _uiManager.ShowLosePanel();
    }
}
