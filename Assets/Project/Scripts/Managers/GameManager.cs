using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject] private BlockManager _blockManager;
    [Inject] private TileManager _tileManager;
    [Inject] private TileGenerator _tileGenerator;
    [Inject] private BoardManager _boardManager;
    [Inject] private MergePlatformsManager _mergePlatformsManager;
    [Inject] private BulletGenerator _bulletGenerator;
    [Inject] private WeaponManager _weaponManager;
    [Inject] private EnemySpawner _enemySpawner;
    [Inject] private WaveManager _waveManager;
    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        _tileGenerator.GenerateGrid(6, 6);
        _tileManager.Initialize();
        _bulletGenerator.Initialize();
        _waveManager.Initialize();
        _effectChanceManager.RegisterEffectChance(
            new EffectChanceManager.ProjectileEffectChance
            {
                EffectType = OnHitEffectType.Freeze,
                BaseChance = 0.5f
            });
        _effectChanceManager.RegisterEffectChance(
            new EffectChanceManager.ProjectileEffectChance
            {
                EffectType = OnHitEffectType.Fire,
                BaseChance = 0.5f
            });

        StartCoroutine(_bulletGenerator.StartBulletGeneration());
    }
}