using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    [Header("Player")]
    [SerializeField] private PlayerRuntimeManager playerManager;

    [Header("UI")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AbilityPanel abilityPanel;
    [SerializeField] private PauseMenuPanel pauseMenuPanel;

    [Header("Board-Related Managers")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private TileGenerator tileGenerator;
    [SerializeField] private BlockManager blockManager;
    [SerializeField] private MergePlatformsManager mergePlatformsManager;

    [Header("Weapons & Bullets")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private BulletGenerator bulletGenerator;
    [SerializeField] private BulletsStack bulletsStack;

    [Header("Effects Managers")]
    [SerializeField] private EffectFactory effectFactory;
    [SerializeField] private EffectRulesConfigSO effectRulesConfigSO;
    [SerializeField] private LightningManager lightningManager;

    [Header("Enemy-Related Scripts")]
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private WaveManager waveManager;

    public override void InstallBindings()
    {
        //Player
        Container.Bind<PlayerRuntimeManager>().FromInstance(playerManager).AsSingle();

        //UI
        Container.Bind<UIManager>().FromInstance(uiManager).AsSingle();
        Container.Bind<AbilityPanel>().FromInstance(abilityPanel).AsSingle();
        Container.Bind<PauseMenuPanel>().FromInstance(pauseMenuPanel).AsSingle();

        //Board-Related Managers
        Container.Bind<BoardManager>().FromInstance(boardManager).AsSingle();
        Container.Bind<TileManager>().FromInstance(tileManager).AsSingle();
        Container.Bind<TileGenerator>().FromInstance(tileGenerator).AsSingle();
        Container.Bind<BlockManager>().FromInstance(blockManager).AsSingle();
        Container.Bind<MergePlatformsManager>().FromInstance(mergePlatformsManager).AsSingle();

        //Weapons & Bullets
        Container.Bind<WeaponManager>().FromInstance(weaponManager).AsSingle();
        Container.Bind<BulletGenerator>().FromInstance(bulletGenerator).AsSingle();
        Container.Bind<BulletsStack>().FromInstance(bulletsStack).AsSingle();
        Container.Bind<BulletManager>().AsSingle();
        Container.Bind<EffectChanceManager>().AsSingle();

        //Effects Managers
        Container.Bind<EffectFactory>().FromInstance(effectFactory).AsSingle();
        Container.Bind<EffectRulesConfigSO>().FromInstance(effectRulesConfigSO).AsSingle();
        Container.Bind<LightningManager>().FromInstance(lightningManager).AsSingle();

        //Enemy-Related Scripts
        Container.Bind<EnemyManager>().FromInstance(enemyManager).AsSingle();
        Container.Bind<EnemySpawner>().FromInstance(enemySpawner).AsSingle();
        Container.Bind<WaveManager>().FromInstance(waveManager).AsSingle();

        //Main Camera
        Container.Bind<Camera>().FromInstance(Camera.main).AsSingle();
    }
}
