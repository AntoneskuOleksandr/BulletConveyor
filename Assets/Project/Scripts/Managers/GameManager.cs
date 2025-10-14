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

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        _tileGenerator.GenerateGrid(6, 6);
        _tileManager.Initialize();
        _weaponManager.InitializeWeapons();
        _bulletGenerator.Initialize();
        _waveManager.Initialize();

        StartCoroutine(_bulletGenerator.StartBulletGeneration());

        _weaponManager.OnAllWeaponsDestroyed += Lose;
        _waveManager.OnWaveCompleted += wave =>
        {
            if (wave >= _waveManager.TotalWaves)
                Win();
        };
    }

    private void Win()
    {
        Time.timeScale = 0f;
        _uiManager.ShowWinPanel();
    }

    private void Lose()
    {
        Time.timeScale = 0f;
        _uiManager.ShowLosePanel();
    }
}