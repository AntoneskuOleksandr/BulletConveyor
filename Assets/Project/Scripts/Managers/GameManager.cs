using System.Collections;
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

        _weaponManager.OnAllWeaponsDestroyed += () => StartCoroutine(Lose());
        _waveManager.OnWaveCompleted += wave =>
        {
            if (wave >= _waveManager.TotalWaves)
                StartCoroutine(Win());
        };
    }

    private IEnumerator Win()
    {
        yield return null; // Wait one frame to ensure all events are processed
        Time.timeScale = 0f;
        _uiManager.ShowWinPanel();
    }

    private IEnumerator Lose()
    {
        yield return null; // Wait one frame to ensure all events are processed
        Time.timeScale = 0f;
        _uiManager.ShowLosePanel();
    }
}